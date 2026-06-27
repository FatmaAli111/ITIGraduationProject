using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOs.Design;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ITIGraduationProject.Application.Interfaces.IServices.StudioServices;

namespace ITIGraduationProject.Application.Features.Studio.Commands.CreateDesign;

public class CreateDesignCommandHandler : IRequestHandler<CreateDesignCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPriceCalculation _priceCalculationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<CreateDesignCommandHandler> _logger;

    public CreateDesignCommandHandler(
        IUnitOfWork unitOfWork,
        IPriceCalculation priceCalculationService,
        ICurrentUserService currentUserService,
        IWebHostEnvironment webHostEnvironment,
        ILogger<CreateDesignCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _priceCalculationService = priceCalculationService;
        _currentUserService = currentUserService;
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateDesignCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        _logger.LogInformation("Save started: User identified as {UserId}", userId);

        var webRootPath = _webHostEnvironment.WebRootPath;
        if (string.IsNullOrEmpty(webRootPath))
        {
            webRootPath = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
        }

        Design? design = null;
        bool isNew = false;

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            design = await _unitOfWork.Designs.GetWithImagesAndAssetsAsync(request.Id.Value);
            if (design != null && design.UserId != userId)
            {
                design = null;
            }
        }

        if (design == null)
        {
            isNew = true;
            var designId = (request.Id.HasValue && request.Id.Value != Guid.Empty) ? request.Id.Value : Guid.NewGuid();
            design = new Design
            {
                Id = designId,
                UserId = userId,
                Status = DesignStatus.Draft
            };
            await _unitOfWork.Designs.AddAsync(design);
            _logger.LogInformation("Design created: initialized new Design ID {DesignId}", design.Id);
        }
        else
        {
            _logger.LogInformation("Design identified: reusing existing Design ID {DesignId}", design.Id);
        }

        var oldCanvasState = design.CanvasStateJSON;
        var oldColor = design.SelectedColor;
        var oldSize = design.SelectedSize;
        var oldFabric = design.SelectedFabric;
        var oldPrintMethod = design.SelectedPrintMethod;

        var canvasStateJson = request.CanvasStateJSON;
        var imageUrls = new List<string>();
        var writtenFiles = new List<string>();

        int frontObjectsCount = 0;
        int backObjectsCount = 0;
        int imageObjectsCount = 0;

        try
        {
            // Parse canvas state to extract image sources and persist new base64 uploads
            if (!string.IsNullOrEmpty(canvasStateJson))
            {
                try
                {
                    var rootNode = JsonNode.Parse(canvasStateJson);
                    var sides = new[] { "front", "back" };

                    foreach (var sideName in sides)
                    {
                        var sideNode = rootNode?[sideName];
                        var objectsArray = sideNode?["objects"]?.AsArray();
                        if (objectsArray == null) continue;

                        if (sideName == "front") frontObjectsCount = objectsArray.Count;
                        else if (sideName == "back") backObjectsCount = objectsArray.Count;

                        foreach (var obj in objectsArray)
                        {
                            if (obj == null) continue;
                            var type = obj["type"]?.ToString();
                            if (string.Equals(type, "image", StringComparison.OrdinalIgnoreCase))
                            {
                                imageObjectsCount++;
                                var src = obj["src"]?.ToString();
                                if (!string.IsNullOrEmpty(src))
                                {
                                    if (src.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
                                    {
                                        _logger.LogInformation("[DesignGraphicAssets] Found base64 uploaded graphic asset. Writing to disk...");
                                        var relativePath = await SaveBase64ImageAsync(src, userId, webRootPath);
                                        obj["src"] = relativePath;
                                        imageUrls.Add(relativePath);
                                        writtenFiles.Add(Path.Combine(webRootPath, relativePath.TrimStart('/')));
                                    }
                                    else
                                    {
                                        imageUrls.Add(src);
                                    }
                                }
                            }
                        }
                    }

                    canvasStateJson = rootNode?.ToJsonString() ?? request.CanvasStateJSON;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[DesignGraphicAssets] Error parsing CanvasStateJSON for base64 assets.");
                }
            }

            _logger.LogInformation(
                "[DesignGraphicAssets] Check 1 - CanvasStateJSON Length: {Length}, Front Objects: {FrontCount}, Back Objects: {BackCount}, Image Objects Found: {ImageCount}",
                canvasStateJson?.Length ?? 0, frontObjectsCount, backObjectsCount, imageObjectsCount);

            // Resolve and link graphic assets
            var uniqueUrls = imageUrls.Distinct().ToList();
            var assetsToLink = new List<GraphicAsset>();

            _logger.LogInformation(
                "[DesignGraphicAssets] Design {DesignId}: found {ImageCount} distinct image URL(s) in CanvasStateJSON.",
                design.Id, uniqueUrls.Count);

            foreach (var url in uniqueUrls)
            {
                // Use tracking query so EF Core recognises the entity in its identity map.
                var asset = await _unitOfWork.GraphicAssets
                    .GetTableAsTracking()
                    .FirstOrDefaultAsync(ga => ga.ImageUrl == url && ga.UserId == userId, cancellationToken);

                bool existsInDb = asset != null;
                if (!existsInDb)
                {
                    _logger.LogInformation(
                        "[DesignGraphicAssets] Check 2 - Image url: {Url} -> NOT FOUND in DB. Creating new GraphicAsset record.", url);
                    var fileName = Path.GetFileName(url);
                    asset = new GraphicAsset
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Name = fileName ?? "Uploaded Graphic",
                        Type = GraphicAssetType.UploadedImage,
                        ImageUrl = url,
                        Tags = "studio,upload"
                    };
                    await _unitOfWork.GraphicAssets.AddAsync(asset);
                }
                else
                {
                    _logger.LogInformation(
                        "[DesignGraphicAssets] Check 2 - Image url: {Url} -> FOUND in DB. GraphicAsset ID: {AssetId}", url, asset!.Id);
                }

                assetsToLink.Add(asset);
            }

            _logger.LogInformation(
                "[DesignGraphicAssets] Design {DesignId}: resolved {AssetCount} GraphicAsset(s): [{AssetIds}]",
                design.Id,
                assetsToLink.Count,
                string.Join(", ", assetsToLink.Select(a => a.Id)));

            // Use the repository method that correctly manages EF Core join-table rows,
            // whether the Design is brand-new (EntityState.Added) or an existing one.
            await _unitOfWork.Designs.SetGraphicAssetsAsync(design, assetsToLink, cancellationToken);

            _logger.LogInformation(
                "[DesignGraphicAssets] Check 3 - Design ID: {DesignId}, GraphicAssets Count: {Count} before SaveChanges.",
                design.Id, design.GraphicAssets.Count);
            foreach (var asset in design.GraphicAssets)
            {
                _logger.LogInformation(
                    "[DesignGraphicAssets] Check 3 - Linked asset: ID = {AssetId}, ImageUrl = {Url}",
                    asset.Id, asset.ImageUrl);
            }


            design.ProductId = request.ProductId;
            if (request.TemplateId.HasValue)
            {
                design.TemplateId = request.TemplateId.Value;
            }
            design.CanvasStateJSON = canvasStateJson;
            design.SelectedColor = request.SelectedColor;
            design.SelectedSize = request.SelectedSize;
            design.SelectedFabric = request.SelectedFabric;
            design.SelectedPrintMethod = request.SelectedPrintMethod;

            design.CalculatedPrice = await _priceCalculationService.CalculatePriceAsync(
                request.ProductId,
                request.SelectedFabric,
                request.SelectedPrintMethod,
                request.SelectedSize,
                cancellationToken
            );

            // Determine if the design state has changed significantly to regenerate snapshots
            bool stateChanged = isNew ||
                                oldCanvasState != canvasStateJson ||
                                oldColor != request.SelectedColor ||
                                oldSize != request.SelectedSize ||
                                oldFabric != request.SelectedFabric ||
                                oldPrintMethod != request.SelectedPrintMethod;

            if (stateChanged)
            {
                _logger.LogInformation("State changed: writing snapshots to disk...");

                if (!string.IsNullOrEmpty(request.Base64Snapshot))
                {
                    var snapshotUrl = await SaveSnapshotAsync(request.Base64Snapshot, userId, design.Id, "snapshot.png", webRootPath);
                    design.SnapshotImageURL = snapshotUrl;
                    writtenFiles.Add(Path.Combine(webRootPath, snapshotUrl.TrimStart('/')));
                    _logger.LogInformation("Snapshot generated: relative path {Path}", snapshotUrl);
                }

                if (!string.IsNullOrEmpty(request.Base64Front))
                {
                    var frontPath = await SaveSnapshotAsync(request.Base64Front, userId, design.Id, "front.png", webRootPath);
                    var frontImage = design.DesignImages.FirstOrDefault(di => di.IsPrimary);
                    if (frontImage == null)
                    {
                        frontImage = new DesignImage { Id = Guid.NewGuid(), DesignId = design.Id, IsPrimary = true };
                        design.DesignImages.Add(frontImage);
                    }
                    frontImage.ImageUrl = frontPath;
                    writtenFiles.Add(Path.Combine(webRootPath, frontPath.TrimStart('/')));
                    _logger.LogInformation("Front snapshot generated: relative path {Path}", frontPath);
                }

                if (!string.IsNullOrEmpty(request.Base64Back))
                {
                    var backPath = await SaveSnapshotAsync(request.Base64Back, userId, design.Id, "back.png", webRootPath);
                    var backImage = design.DesignImages.FirstOrDefault(di => !di.IsPrimary);
                    if (backImage == null)
                    {
                        backImage = new DesignImage { Id = Guid.NewGuid(), DesignId = design.Id, IsPrimary = false };
                        design.DesignImages.Add(backImage);
                    }
                    backImage.ImageUrl = backPath;
                    writtenFiles.Add(Path.Combine(webRootPath, backPath.TrimStart('/')));
                    _logger.LogInformation("Back snapshot generated: relative path {Path}", backPath);
                }
            }
            else
            {
                _logger.LogInformation("No changes detected in canvas state or customization properties. Reusing existing snapshot files.");
            }

            // Check 6 - ChangeTracker state immediately before SaveChanges
            var changeTrackerStates = _unitOfWork.Designs.GetChangeTrackerState();
            _logger.LogInformation(
                "[DesignGraphicAssets] Check 6 - Tracked Entities in ChangeTracker immediately before SaveChanges (Count: {TrackerCount}):",
                changeTrackerStates.Count);
            foreach (var trackerState in changeTrackerStates)
            {
                _logger.LogInformation("[DesignGraphicAssets] Check 6 - {TrackerState}", trackerState);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "[DesignGraphicAssets] SaveChanges complete. Design {DesignId} saved successfully.",
                design.Id);

            // Check 7 - Query database immediately to verify SQL rows exist
            var sqlRowsCount = await _unitOfWork.Designs.GetDesignGraphicAssetsCountAsync(design.Id, cancellationToken);
            _logger.LogInformation(
                "[DesignGraphicAssets] Check 7 - SQL Verification: DesignGraphicAssets table count for Design {DesignId} = {RowCount} row(s) returned from database.",
                design.Id, sqlRowsCount);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Save failed: cleaning up any written files...");
            foreach (var file in writtenFiles)
            {
                if (File.Exists(file))
                {
                    try { File.Delete(file); } catch { /* ignore */ }
                }
            }
            throw;
        }

        return design.Id;
    }

    private async Task<string> SaveBase64ImageAsync(string base64DataUrl, Guid userId, string webRootPath)
    {
        var match = Regex.Match(base64DataUrl, @"^data:(image/[a-zA-Z0-9+-]+);base64,(.+)");
        if (!match.Success)
        {
            throw new ArgumentException("Invalid base64 image data URL format.");
        }

        var mimeType = match.Groups[1].Value;
        var base64Data = match.Groups[2].Value;
        var bytes = Convert.FromBase64String(base64Data);

        var extension = mimeType switch
        {
            "image/png" => ".png",
            "image/jpeg" or "image/jpg" => ".jpg",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            _ => ".png"
        };

        var assetGuid = Guid.NewGuid();
        var fileName = $"{assetGuid}{extension}";
        var userFolder = Path.Combine(webRootPath, "GraphicAssets", userId.ToString());
        
        if (!Directory.Exists(userFolder))
        {
            Directory.CreateDirectory(userFolder);
        }

        var filePath = Path.Combine(userFolder, fileName);
        await File.WriteAllBytesAsync(filePath, bytes);

        return $"/GraphicAssets/{userId}/{fileName}";
    }

    private async Task<string> SaveSnapshotAsync(string base64DataUrl, Guid userId, Guid designId, string fileName, string webRootPath)
    {
        var base64Data = base64DataUrl;
        if (base64DataUrl.Contains(","))
        {
            base64Data = base64DataUrl.Split(',')[1];
        }

        var bytes = Convert.FromBase64String(base64Data);
        var folder = Path.Combine(webRootPath, "DesignSnapshots", userId.ToString(), designId.ToString());
        
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var filePath = Path.Combine(folder, fileName);
        await File.WriteAllBytesAsync(filePath, bytes);

        return $"/DesignSnapshots/{userId}/{designId}/{fileName}";
    }
}
