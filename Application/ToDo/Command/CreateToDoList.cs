using Application.Core;
using AutoMapper;
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
            /** Langkah 1: Memetakan DTO ToDoList ke entitas ToDoList menggunakan AutoMapper **/
            var toDoList = _mapper.Map<ToDoList>(request.ToDoListDto);

            /** Langkah 2: Menambahkan entitas ToDoList ke konteks database **/
            _context.ToDoLists.Add(toDoList);

            /** Langkah 3: Menyimpan perubahan ke database **/
            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            /** Langkah 4: Memeriksa apakah penyimpanan berhasil **/
            if (!result)
                return Result<ToDoListDto>.Failure("Gagal untuk membuat ToDoList");

            /** Langkah 5: Memetakan entitas ToDoList kembali ke DTO ToDoListDto menggunakan AutoMapper **/
            var toDoListDto = _mapper.Map<ToDoListDto>(toDoList);

            /** Langkah 6: Mengembalikan hasil berhasil dengan DTO ToDoListDto **/
            return Result<ToDoListDto>.Success(toDoListDto);
        }
    }
}

public class CommandValidator : AbstractValidator<ToDoListDto>
{
    public CommandValidator()
    {
        RuleFor(x => x.Description).NotEmpty().WithMessage("Deskripsi tidak boleh kosong");
    }
}