using ConversionReportService.Presentation.Grpc.Interceptors;
using Grpc.Core;
using Xunit;

namespace ConversionReportService.Tests.Presentation;

public class GrpcExceptionInterceptorTests
{
    [Fact]
    public async Task UnaryServerHandler_ShouldMapKeyNotFound_ToNotFound()
    {
        // Arrange
        var interceptor = new GrpcExceptionInterceptor();
        var context = CreateContext();

        // Act
        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            interceptor.UnaryServerHandler<string, string>(
                "req",
                context,
                (_, _) => throw new KeyNotFoundException("missing")));

        // Assert
        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task UnaryServerHandler_ShouldMapArgumentException_ToInvalidArgument()
    {
        // Arrange
        var interceptor = new GrpcExceptionInterceptor();
        var context = CreateContext();

        // Act
        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            interceptor.UnaryServerHandler<string, string>(
                "req",
                context,
                (_, _) => throw new ArgumentException("invalid")));

        // Assert
        Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
    }

    private static ServerCallContext CreateContext()
    {
        return new FakeServerCallContext();
    }

    private sealed class FakeServerCallContext : ServerCallContext
    {
        protected override string MethodCore => "test/method";
        protected override string HostCore => "localhost";
        protected override string PeerCore => "127.0.0.1";
        protected override DateTime DeadlineCore => DateTime.UtcNow.AddMinutes(1);
        protected override Metadata RequestHeadersCore => new();
        protected override CancellationToken CancellationTokenCore => CancellationToken.None;
        protected override Metadata ResponseTrailersCore { get; } = new();
        protected override Status StatusCore { get; set; }
        protected override WriteOptions? WriteOptionsCore { get; set; }
        protected override AuthContext AuthContextCore => new(string.Empty, new Dictionary<string, List<AuthProperty>>());

        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options)
        {
            throw new NotSupportedException();
        }

        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
        {
            return Task.CompletedTask;
        }
    }
}
