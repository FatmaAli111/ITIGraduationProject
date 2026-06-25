using MediatR;

namespace ITIGraduationProject.Application.Features.Studio.Commands.GenerateAiImage
{
    /// <summary>
    /// Command received from the Angular Design Studio chat.
    /// The UserId is never accepted from the frontend; it is resolved from JWT inside the handler.
    /// </summary>
    public record GenerateAiImageCommand(string Prompt) : IRequest<GenerateAiImageResult>;

    /// <summary>
    /// Response returned to the Angular client after a successful AI image generation.
    /// ImageUrl references the locally stored file inside wwwroot/GraphicAssets/{UserId}/.
    /// </summary>
    public record GenerateAiImageResult(Guid GraphicAssetId, string ImageUrl);
}
