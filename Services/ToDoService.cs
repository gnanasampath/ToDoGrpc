using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ToDoGrpc.Data;
using ToDoGrpc.Models;
using ToDoGrpc.Protos;

namespace ToDoGrpc.Services;

public class ToDoService : ToDoIt.ToDoItBase
{
    private readonly AppDbContext _dbContext;

    public ToDoService(AppDbContext appDbContext)
    {
        _dbContext = appDbContext;
    }

    public override async Task<CreateToDoResponse> CreateToDo(CreateToDoRequest request, ServerCallContext serverCallContext)
    {

        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply id greater than 0"));
        }
        var toDoItem = new ToDoItem
        {
            Title = request.Title,
            Description = request.Description
        };

        await _dbContext.AddAsync(toDoItem);
        await _dbContext.SaveChangesAsync();
        return await Task.FromResult(new CreateToDoResponse
        {
            Id = toDoItem.id
        });
    }

    public override async Task<ReadToDoResponse> ReadToDo(ReadToDoRequest request, ServerCallContext context)
    {
        if (request.Id <= 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply valid id"));
        }

        var toDoItem = await _dbContext.ToDoitems.FirstOrDefaultAsync(t => t.id == request.Id);

        if (toDoItem != null)
        {
            return await Task.FromResult(new ReadToDoResponse
            {
                Id = toDoItem.id,
                Description = toDoItem.Description,
                Title = toDoItem.Title,
                ToDoStatus = toDoItem.ToDoStatus
            });
        }

        throw new RpcException(new Status(StatusCode.NotFound, $"No task with id {request.Id}"));
    }

    public override async Task<GetAllResponse> ListToDo(GetAllRequest request, ServerCallContext context)
    {
        var response = new GetAllResponse();
        var toDoItems = await _dbContext.ToDoitems.ToListAsync();

        foreach (var toDoItem in toDoItems)
        {
            response.ToDo.Add(new ReadToDoResponse
            {
                Id = toDoItem.id,
                Description = toDoItem.Description,
                Title = toDoItem.Title,
                ToDoStatus = toDoItem.ToDoStatus
            });

        }

        return await Task.FromResult(response);
    }

    public override async Task<UpdateToDoResponse> UpdateToDo(UpdateToDoRequest request, ServerCallContext context)
    {
        if (request.Id <= 0 || string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply valid object"));
        }

        var toDoItem = await _dbContext.ToDoitems.FirstOrDefaultAsync(t => t.id == request.Id);

        if (toDoItem == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"No task with id {request.Id}"));

        toDoItem.ToDoStatus = request.ToDoStatus;
        toDoItem.Title = request.Title;
        toDoItem.Description = request.Description;

        await _dbContext.SaveChangesAsync();

        return await Task.FromResult(new UpdateToDoResponse { Id = toDoItem.id });
    }

    public override async Task<DeleteToDoResponse> DeleteToDo(DeleteToDoRequest request, ServerCallContext context)
    {
        if (request.Id <= 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply valid id"));
        }

        var toDoItem = await _dbContext.ToDoitems.FirstOrDefaultAsync(t => t.id == request.Id);

        if (toDoItem == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"No task with id {request.Id}"));


        _dbContext.Remove(toDoItem);
        await _dbContext.SaveChangesAsync();

        return await Task.FromResult(new DeleteToDoResponse { Id = toDoItem.id });
    }
}