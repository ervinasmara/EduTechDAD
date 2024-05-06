using Application.ClassRooms;
using Application.Core;
using Application.Learn.Lessons;
using AutoMapper;
using Domain.Class;
using Domain.Learn.Lessons;
using Domain.ToDoList;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.ToDo.Command;
public class CreateToDoList
{
    public class Command : IRequest<Result<ToDoListDto>>
    {
        public ToDoListDto ToDoListDto { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<ToDoListDto>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<ToDoListDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var toDoList = _mapper.Map<ToDoList>(request.ToDoListDto);

            _context.ToDoLists.Add(toDoList);

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
                return Result<ToDoListDto>.Failure("Failed to Create ToDoList");

            var toDoListDto = _mapper.Map<ToDoListDto>(toDoList);

            return Result<ToDoListDto>.Success(toDoListDto);
        }
    }
}

public class CommandValidator : AbstractValidator<ToDoListDto>
{
    public CommandValidator()
    {
        RuleFor(x => x.Description).NotEmpty();
    }
}