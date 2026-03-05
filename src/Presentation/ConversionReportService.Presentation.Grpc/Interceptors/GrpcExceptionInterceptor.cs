using Grpc.Core;
using Grpc.Core.Interceptors;

namespace ConversionReportService.Presentation.Grpc.Interceptors;

public sealed class GrpcExceptionInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new RpcException(
                new Status(StatusCode.PermissionDenied, ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            throw new RpcException(
                new Status(StatusCode.NotFound, ex.Message));
        }
        catch (ArgumentNullException ex)
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (ArgumentException ex)
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(
                new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (NotSupportedException ex)
        {
            throw new RpcException(
                new Status(StatusCode.Unimplemented, ex.Message));
        }
        catch (Exception ex)
        {
            throw new RpcException(
                new Status(StatusCode.Internal, $"Unhandled server error: {ex.Message}"));
        }
    }
}