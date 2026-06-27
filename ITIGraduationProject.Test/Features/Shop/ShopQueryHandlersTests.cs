using ITIGraduationProject.Application.Common.Mappings;
using ITIGraduationProject.Application.DTOS.ShopDTOs;
using ITIGraduationProject.Application.Features.Shop.Queries.Handlers;
using ITIGraduationProject.Application.Features.Shop.Queries.Models;
using ITIGraduationProject.Application.Interfaces.IRepositories;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Interfaces.Repositories;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Enums;
using Mapster;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace ITIGraduationProject.Test.Features.Shop;

[TestFixture]
public class ShopQueryHandlersTests
{
    private Mock<IUnitOfWork> _uow = null!;
    private Mock<IProductRepository> _productsRepo = null!;
    private Mock<IProductImageRepository> _productImagesRepo = null!;
    private Mock<ICategoryRepository> _categoriesRepo = null!;

    private GetCategoriesQueryHandler _categoriesHandler = null!;
    private GetProductByIdQueryHandler _productByIdHandler = null!;
    private GetProductImagesQueryHandler _productImagesHandler = null!;
    private GetProductsQueryHandler _productsHandler = null!;

    [OneTimeSetUp]
    public void RegisterMapster()
    {
        new StudioMappingConfig().Register(TypeAdapterConfig.GlobalSettings);

        TypeAdapterConfig.GlobalSettings.NewConfig<Category, CategoryDto>();
        TypeAdapterConfig.GlobalSettings.NewConfig<ProductImage, ProductImageDto>();

        TypeAdapterConfig.GlobalSettings.Compile();
    }

    [SetUp]
    public void Setup()
    {
        _uow = new Mock<IUnitOfWork>();
        _productsRepo = new Mock<IProductRepository>();
        _productImagesRepo = new Mock<IProductImageRepository>();
        _categoriesRepo = new Mock<ICategoryRepository>();

        _uow.Setup(x => x.Products).Returns(_productsRepo.Object);
        _uow.Setup(x => x.ProductImages).Returns(_productImagesRepo.Object);
        _uow.Setup(x => x.Categories).Returns(_categoriesRepo.Object);

        _categoriesHandler = new GetCategoriesQueryHandler(_uow.Object);
        _productByIdHandler = new GetProductByIdQueryHandler(_uow.Object);
        _productImagesHandler = new GetProductImagesQueryHandler(_uow.Object);
        _productsHandler = new GetProductsQueryHandler(_uow.Object);
    }

    [Test]
    public async Task GetCategories_Should_Return_All_NonDeleted_Categories()
    {
        var category1 = new Category { Id = Guid.NewGuid(), Name = "T-Shirts" };
        var category2 = new Category { Id = Guid.NewGuid(), Name = "Mugs" };
        var categories = new List<Category>
        {
            category1,
            category2,
            new() { Id = Guid.NewGuid(), Name = "Deleted", IsDeleted = true }
        };

        _categoriesRepo.Setup(x => x.GetTableNoTracking())
            .Returns(categories.AsQueryable().BuildMock());

        var result = await _categoriesHandler.Handle(new GetCategoriesQuery(), CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Count, Is.EqualTo(2));
        Assert.That(result.Data.Select(c => c.Name), Is.EquivalentTo(new[] { "T-Shirts", "Mugs" }));
    }

    [Test]
    public async Task GetProductById_Should_Return_Product()
    {
        var productId = Guid.NewGuid();
        var category = new Category { Id = Guid.NewGuid(), Name = "Posters" };

        var product = new Product
        {
            Id = productId,
            Name = "Art Print",
            BasePrice = 15m,
            Category = category,
            CategoryId = category.Id,
            PreviewImageURL = "/preview.png",
            IsAvailable = true,
            AverageRating = 4.5m,
            ReviewCount = 10,
            IsDeleted = false,
            ProductImages = new List<ProductImage>()
        };

        var products = new List<Product> { product };

        _productsRepo.Setup(x => x.GetTableNoTracking())
            .Returns(products.AsQueryable().BuildMock());

        var result = await _productByIdHandler.Handle(
            new GetProductByIdQuery(productId),
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data!.Id, Is.EqualTo(productId));
        Assert.That(result.Data.Name, Is.EqualTo("Art Print"));
        Assert.That(result.Data.CategoryName, Is.EqualTo("Posters"));
        Assert.That(result.Data.BasePrice, Is.EqualTo(15m));
    }

    [Test]
    public async Task GetProductById_Should_Return_NotFound_When_Product_NotFound()
    {
        _productsRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<Product>().AsQueryable().BuildMock());

        var result = await _productByIdHandler.Handle(
            new GetProductByIdQuery(Guid.NewGuid()),
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message, Is.EqualTo("Product not found"));
    }

    [Test]
    public async Task GetProductImages_Should_Return_Images()
    {
        var productId = Guid.NewGuid();

        var images = new List<ProductImage>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                ImageUrl = "/img1.png",
                DisplayOrder = 2,
                IsPrimary = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                ImageUrl = "/img2.png",
                DisplayOrder = 1,
                IsPrimary = true,
                ViewAngle = ViewAngle.Front
            },
            new()
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ImageUrl = "/other.png",
                DisplayOrder = 1
            }
        };

        _productImagesRepo.Setup(x => x.GetTableNoTracking())
            .Returns(images.AsQueryable().BuildMock());

        var result = await _productImagesHandler.Handle(
            new GetProductImagesQuery(productId),
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data!.Count, Is.EqualTo(2));
        Assert.That(result.Data.All(i => i.ProductId == productId), Is.True);
    }

    [Test]
    public async Task GetProductImages_Should_Return_Empty_When_No_Images()
    {
        var productId = Guid.NewGuid();

        _productImagesRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<ProductImage>().AsQueryable().BuildMock());

        var result = await _productImagesHandler.Handle(
            new GetProductImagesQuery(productId),
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data!, Is.Empty);
    }

    [Test]
    public async Task GetProducts_Should_Return_Products()
    {
        var category = new Category { Id = Guid.NewGuid(), Name = "Apparel" };

        var products = new List<Product>
        {
            CreateProduct("Hoodie", category, 49.99m),
            CreateProduct("Cap", category, 19.99m)
        };

        _productsRepo.Setup(x => x.GetTableNoTracking())
            .Returns(products.AsQueryable().BuildMock());

        var result = await _productsHandler.Handle(
            new GetProductsQuery(1, 10),
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data!.Data.Count, Is.EqualTo(2));
        Assert.That(result.Data.TotalCount, Is.EqualTo(2));
    }

    [Test]
    public async Task GetProducts_Should_Filter_By_Category()
    {
        var category1 = new Category { Id = Guid.NewGuid(), Name = "Apparel" };
        var category2 = new Category { Id = Guid.NewGuid(), Name = "Accessories" };

        var products = new List<Product>
        {
            CreateProduct("Hoodie", category1, 49.99m),
            CreateProduct("Cap", category1, 19.99m),
            CreateProduct("Mug", category2, 9.99m)
        };

        _productsRepo.Setup(x => x.GetTableNoTracking())
            .Returns(products.AsQueryable().BuildMock());

        var result = await _productsHandler.Handle(
            new GetProductsQuery(1, 10, category1.Id),
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data!.Data.Count, Is.EqualTo(2));
        Assert.That(result.Data.Data.All(p => p.CategoryName == "Apparel"), Is.True);
    }

    [Test]
    public async Task GetProducts_Should_Apply_Pagination()
    {
        var category = new Category { Id = Guid.NewGuid(), Name = "Prints" };

        var products = Enumerable.Range(1, 15)
            .Select(i => CreateProduct($"Print {i}", category, i * 5m))
            .ToList();

        _productsRepo.Setup(x => x.GetTableNoTracking())
            .Returns(products.AsQueryable().BuildMock());

        var page1 = await _productsHandler.Handle(
            new GetProductsQuery(1, 10),
            CancellationToken.None);

        var page2 = await _productsHandler.Handle(
            new GetProductsQuery(2, 10),
            CancellationToken.None);

        Assert.That(page1.Succeeded, Is.True);
        Assert.That(page1.Data!.Data.Count, Is.EqualTo(10));
        Assert.That(page1.Data.TotalCount, Is.EqualTo(15));
        Assert.That(page1.Data.TotalPages, Is.EqualTo(2));

        Assert.That(page2.Succeeded, Is.True);
        Assert.That(page2.Data!.Data.Count, Is.EqualTo(5));
        Assert.That(page2.Data.CurrentPage, Is.EqualTo(2));
    }

    private static Product CreateProduct(string name, Category category, decimal price)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            BasePrice = price,
            Category = category,
            CategoryId = category.Id,
            PreviewImageURL = "/preview.png",
            IsAvailable = true,
            AverageRating = 0,
            ReviewCount = 0,
            IsDeleted = false,
            ProductImages = new List<ProductImage>()
        };
    }
}
