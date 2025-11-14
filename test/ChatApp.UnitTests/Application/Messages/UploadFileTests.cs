using ChatApp.Application.Abstractions.Storage;
using ChatApp.Application.UseCases.Messages.UploadFile;

using FluentAssertions;

using NSubstitute;

namespace ChatApp.UnitTests.Application.Messages;

public class UploadFileTests
{
    private readonly UploadFileCommandHandler _handler;
    private readonly IFileStorageService _fileStorageServiceMock;

    public UploadFileTests()
    {
        _fileStorageServiceMock = Substitute.For<IFileStorageService>();

        _handler = new UploadFileCommandHandler(_fileStorageServiceMock);
    }

    [Fact]
    public async Task Handle_Deve_Upload_Arquivo_Com_Sucesso()
    {
        // Arrange
        var fileName = "test.jpg";
        var contentType = "image/jpeg";
        var extension = ".jpg";
        var fileContent = new MemoryStream([1, 2, 3, 4]);
        var expectedUrl = "https://s3.amazonaws.com/bucket/messages/guid.jpg";

        var command = new UploadFileCommand(fileName, contentType, fileContent, extension);

        _fileStorageServiceMock.GeneratePresignedUrl(Arg.Any<string>(), Arg.Any<TimeSpan>())
            .Returns(expectedUrl);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FileUrl.Should().Be(expectedUrl);

        await _fileStorageServiceMock.Received(1).Upload(
            fileContent,
            Arg.Is<string>(k => k.StartsWith("messages/") && k.EndsWith(extension)),
            contentType,
            Arg.Any<CancellationToken>());

        _fileStorageServiceMock.Received(1).GeneratePresignedUrl(
            Arg.Is<string>(k => k.StartsWith("messages/") && k.EndsWith(extension)),
            TimeSpan.FromMinutes(10));
    }

    [Fact]
    public async Task Handle_Deve_Gerar_Chave_Unica_Para_Cada_Upload()
    {
        // Arrange
        var command1 = new UploadFileCommand("test1.jpg", "image/jpeg", new MemoryStream(), ".jpg");
        var command2 = new UploadFileCommand("test2.jpg", "image/jpeg", new MemoryStream(), ".jpg");

        var keys = new List<string>();
        _fileStorageServiceMock.When(x => x.Upload(
                Arg.Any<Stream>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>()))
            .Do(x => keys.Add(x.ArgAt<string>(1))); // key é o segundo parâmetro (índice 1)

        // Act
        await _handler.Handle(command1, CancellationToken.None);
        await _handler.Handle(command2, CancellationToken.None);

        // Assert
        keys.Should().HaveCount(2);
        keys[0].Should().NotBe(keys[1]);
        keys[0].Should().StartWith("messages/");
        keys[1].Should().StartWith("messages/");
    }
}

