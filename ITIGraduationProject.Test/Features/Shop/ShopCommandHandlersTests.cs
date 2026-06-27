using ITIGraduationProject.Application.Features.Shop.Commands.Handlers;
using ITIGraduationProject.Application.Features.Shop.Commands.Models;
using ITIGraduationProject.Application.Interfaces.IRepositories;
using ITIGraduationProject.Application.Interfaces.IServices.FilesServices;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Interfaces.Repositories;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Enums;
using Microsoft.AspNetCore.Http;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using System.Net;

namespace ITIGraduationProject.Test.Features.Shop;

[TestFixture]
public class ShopCommandHandlersTests
{
    private Mock<IUnitOfWork> _uow = null!;
    private Mock<ICategoryRepository> _categoriesRepo = null!;
    private Mock<IProductRepository> _productsRepo = null!;
    private Mock<IProductImageRepository> _productImagesRepo = null!;
    private Mock<IFileService> _fileService = null!;

    private CreateCategoryCommandHandler _createCategoryHandler = null!;
    private CreateProductCommandHandler _createProductHandler = null!;
    private DeleteCategoryCommandHandler _deleteCategoryHandler = null!;
    private DeleteProductCommandHandler _deleteProductHandler = null!;
    private UpdateCategoryCommandHandler _updateCategoryHandler = null!;
    private UpdateProductCommandHandler _updateProductHandler = null!;
    private CreateProductImageCommandHandler _createProductImageHandler = null!;

    [SetUp]
    public void Setup()
    {
        _uow = new Mock<IUnitOfWork>();
        _categoriesRepo = new Mock<ICategoryRepository>();
        _productsRepo = new Mock<IProductRepository>();
        _productImagesRepo = new Mock<IProductImageRepository>();
        _fileService = new Mock<IFileService>();

        _uow.Setup(x => x.Categories).Returns(_categoriesRepo.Object);
        _uow.Setup(x => x.Products).Returns(_productsRepo.Object);
        _uow.Setup(x => x.ProductImages).Returns(_productImagesRepo.Object);

        _createCategoryHandler = new CreateCategoryCommandHandler(_uow.Object);
        _createProductHandler = new CreateProductCommandHandler(_uow.Object);
        _deleteCategoryHandler = new DeleteCategoryCommandHandler(_uow.Object);
        _deleteProductHandler = new DeleteProductCommandHandler(_uow.Object);
        _updateCategoryHandler = new UpdateCategoryCommandHandler(_uow.Object);
        _updateProductHandler = new UpdateProductCommandHandler(_uow.Object);
        _createProductImageHandler = new CreateProductImageCommandHandler(
            _uow.Object,
            _fileService.Object);
    }

    #region CreateCategory

    [Test]
    public async Task CreateCategory_Should_Create_Category()
    {
        _categoriesRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<Category>().AsQueryable().BuildMock());

        _categoriesRepo.Setup(x => x.AddAsync(It.IsAny<Category>()))
            .ReturnsAsync((Category c) => c);

        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var command = new CreateCategoryCommand(
            "T-Shirts",
            "Cotton tees",
            "{}",
            "/images/tshirts.png");

        var result = await _createCategoryHandler.Handle(command, CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(result.Data!.Name, Is.EqualTo("T-Shirts"));

        _categoriesRepo.Verify(x => x.AddAsync(It.Is<Category>(c =>
            c.Name == "T-Shirts" &&
            c.Description == "Cotton tees")), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task CreateCategory_Should_Return_BadRequest_When_Duplicate()
    {
        var existing = new List<Category>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "T-Shirts",
                IsDeleted = false
            }
        };

        _categoriesRepo.Setup(x => x.GetTableNoTracking())
            .Returns(existing.AsQueryable().BuildMock());

        var command = new CreateCategoryCommand(
            "T-Shirts",
            "Duplicate",
            "{}",
            null);

        var result = await _createCategoryHandler.Handle(command, CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message, Is.EqualTo("Category already exists"));

        _categoriesRepo.Verify(x => x.AddAsync(It.IsAny<Category>()), Times.Never);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region CreateProduct

    [Test]
    public async Task CreateProduct_Should_Create_Product()
    {
        var categoryId = Guid.NewGuid();

        _productsRepo.Setup(x => x.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => p);

        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var command = new CreateProductCommand(
            categoryId,
            "Classic Tee",
            29.99m,
            ProductAvailableColors.Black,
            "/preview.png",
            true,
            "InStock");

        var result = await _createProductHandler.Handle(command, CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(result.Data!.Name, Is.EqualTo("Classic Tee"));
        Assert.That(result.Data.BasePrice, Is.EqualTo(29.99m));

        _productsRepo.Verify(x => x.AddAsync(It.Is<Product>(p =>
            p.CategoryId == categoryId &&
            p.Name == "Classic Tee" &&
            p.IsAvailable)), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    #endregion

    #region DeleteCategory

    [Test]
    public async Task DeleteCategory_Should_Delete_Category()
    {
        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            Name = "Hoodies",
            IsDeleted = false
        };

        _categoriesRepo.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync(category);
        _productsRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<Product>().AsQueryable().BuildMock());
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _deleteCategoryHandler.Handle(
            new DeleteCategoryCommand(categoryId),
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(category.IsDeleted, Is.True);
        Assert.That(category.DeletedAt, Is.Not.Null);

        _categoriesRepo.Verify(x => x.Update(category), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeleteCategory_Should_Return_NotFound_When_Category_NotFound()
    {
        var categoryId = Guid.NewGuid();

        _categoriesRepo.Setup(x => x.GetByIdAsync(categoryId))
            .ReturnsAsync((Category?)null);

        var result = await _deleteCategoryHandler.Handle(
            new DeleteCategoryCommand(categoryId),
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message, Is.EqualTo("Category not found"));

        _categoriesRepo.Verify(x => x.Update(It.IsAny<Category>()), Times.Never);
    }

    [Test]
    public async Task DeleteCategory_Should_Return_BadRequest_When_Category_Has_Products()
    {
        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            Name = "Mugs",
            IsDeleted = false
        };

        var products = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CategoryId = categoryId,
                Name = "Coffee Mug",
                IsDeleted = false
            }
        };

        _categoriesRepo.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync(category);
        _productsRepo.Setup(x => x.GetTableNoTracking())
            .Returns(products.AsQueryable().BuildMock());

        var result = await _deleteCategoryHandler.Handle(
            new DeleteCategoryCommand(categoryId),
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message, Is.EqualTo("Cannot delete a category that still has products"));

        _categoriesRepo.Verify(x => x.Update(It.IsAny<Category>()), Times.Never);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region DeleteProduct

    [Test]
    public async Task DeleteProduct_Should_Delete_Product()
    {
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Poster",
            IsDeleted = false
        };

        _productsRepo.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _deleteProductHandler.Handle(
            new DeleteProductCommand(productId),
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(product.IsDeleted, Is.True);
        Assert.That(product.DeletedAt, Is.Not.Null);

        _productsRepo.Verify(x => x.Update(product), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeleteProduct_Should_Return_NotFound_When_Product_NotFound()
    {
        var productId = Guid.NewGuid();

        _productsRepo.Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        var result = await _deleteProductHandler.Handle(
            new DeleteProductCommand(productId),
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message, Is.EqualTo("Product not found"));

        _productsRepo.Verify(x => x.Update(It.IsAny<Product>()), Times.Never);
    }

    #endregion

    #region UpdateCategory

    [Test]
    public async Task UpdateCategory_Should_Update_Category()
    {
        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            Name = "Old Name",
            Description = "Old desc",
            IsDeleted = false
        };

        _categoriesRepo.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync(category);
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var command = new UpdateCategoryCommand(
            categoryId,
            "New Name",
            "New desc",
            "{}",
            "/new.png");

        var result = await _updateCategoryHandler.Handle(command, CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data!.Name, Is.EqualTo("New Name"));
        Assert.That(category.Name, Is.EqualTo("New Name"));
        Assert.That(category.Description, Is.EqualTo("New desc"));
        Assert.That(category.UpdatedAt, Is.Not.Null);

        _categoriesRepo.Verify(x => x.Update(category), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateCategory_Should_Return_NotFound_When_Category_NotFound()
    {
        var categoryId = Guid.NewGuid();

        _categoriesRepo.Setup(x => x.GetByIdAsync(categoryId))
            .ReturnsAsync((Category?)null);

        var result = await _updateCategoryHandler.Handle(
            new UpdateCategoryCommand(categoryId, "Name", null, null, null),
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message, Is.EqualTo("Category not found"));
    }

    #endregion

    #region UpdateProduct

    [Test]
    public async Task UpdateProduct_Should_Update_Product()
    {
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Old Product",
            BasePrice = 10m,
            IsAvailable = false,
            IsDeleted = false
        };

        _productsRepo.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var command = new UpdateProductCommand(
            productId,
            "Updated Product",
            19.99m,
            ProductAvailableColors.Red,
            "/updated.png",
            true,
            "InStock");

        var result = await _updateProductHandler.Handle(command, CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data!.Name, Is.EqualTo("Updated Product"));
        Assert.That(product.Name, Is.EqualTo("Updated Product"));
        Assert.That(product.BasePrice, Is.EqualTo(19.99m));
        Assert.That(product.IsAvailable, Is.True);
        Assert.That(product.UpdatedAt, Is.Not.Null);

        _productsRepo.Verify(x => x.Update(product), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateProduct_Should_Return_NotFound_When_Product_NotFound()
    {
        var productId = Guid.NewGuid();

        _productsRepo.Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        var result = await _updateProductHandler.Handle(
            new UpdateProductCommand(productId, "Name", null, null, null, null, null),
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message, Is.EqualTo("Product not found"));
    }

    #endregion

    #region CreateProductImage

    [Test]
    public async Task CreateProductImage_Should_Create_Image()
    {
        var productId = Guid.NewGuid();
        var products = new List<Product>
        {
            new() { Id = productId, Name = "Tee", IsDeleted = false }
        };

        _productsRepo.Setup(x => x.GetTableNoTracking())
            .Returns(products.AsQueryable().BuildMock());

        _productImagesRepo.Setup(x => x.GetTableAsTracking())
            .Returns(new List<ProductImage>().AsQueryable().BuildMock());

        _fileService.Setup(x => x.UploadFileAsync(It.IsAny<IFormFile>(), "products"))
            .ReturnsAsync("/uploads/products/image.png");

        _productImagesRepo.Setup(x => x.AddAsync(It.IsAny<ProductImage>()))
            .ReturnsAsync((ProductImage img) => img);

        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("photo.png");
        fileMock.Setup(f => f.Length).Returns(1024);

        var command = new CreateProductImageCommand(
            productId,
            fileMock.Object,
            (int)ProductAvailableColors.Black,
            (int)ViewAngle.Front,
            "{}",
            true,
            1);

        var result = await _createProductImageHandler.Handle(command, CancellationToken.None);

        Assert.That(result, Is.Not.EqualTo(Guid.Empty));

        _fileService.Verify(x => x.UploadFileAsync(fileMock.Object, "products"), Times.Once);
        _productImagesRepo.Verify(x => x.AddAsync(It.Is<ProductImage>(img =>
            img.ProductId == productId &&
            img.ImageUrl == "/uploads/products/image.png" &&
            img.IsPrimary)), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void CreateProductImage_Should_Throw_When_Product_NotFound()
    {
        var productId = Guid.NewGuid();

        _productsRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<Product>().AsQueryable().BuildMock());

        var fileMock = new Mock<IFormFile>();

        var command = new CreateProductImageCommand(
            productId,
            fileMock.Object,
            null,
            null,
            null,
            false,
            0);

        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _createProductImageHandler.Handle(command, CancellationToken.None));

        _fileService.Verify(x => x.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        _productImagesRepo.Verify(x => x.AddAsync(It.IsAny<ProductImage>()), Times.Never);
    }

    #endregion
}
