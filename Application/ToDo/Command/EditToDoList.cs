using Application.Core;
using Application.Learn.Lessons;
using AutoMapper;
using Domain.Learn.Lessons;
using Domain.ToDoList;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.ToDo.Command;
public class EditToDoList
{
    public class Command : IRequest<Result<ToDoListDto>>
    {
        public Guid ToDoListId { get; set; } // Id toDoList yang akan diedit
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
            var toDoList = await _context.ToDoLists
                .FirstOrDefaultAsync(l => l.Id == request.ToDoListId, cancellationToken);

            if (toDoList == null)
            {
                return Result<ToDoListDto>.Failure("ToDoList tidak ditemukan");
            }

            /** Langkah 2: Update properti ToDoList **/
            //toDoList.Description = request.ToDoListDto.Description;
            _mapper.Map(request.ToDoListDto, toDoList);

            /** Langkah 3: Menyimpan perubahan ke database **/
            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            /** Langkah 4: Memeriksa apakah penyimpanan berhasil **/
            if (!result)
                return Result<ToDoListDto>.Failure("Gagal untuk edit ToDoList");

            /** Langkah 5: Memetakan entitas ToDoList kembali ke DTO ToDoListDto menggunakan AutoMapper **/
            var toDoListDto = _mapper.Map<ToDoListDto>(toDoList);

            /** Langkah 6: Mengembalikan hasil berhasil dengan DTO ToDoListDto **/
            return Result<ToDoListDto>.Success(toDoListDto);
        }
    }
}

public class CommandEditValidator : AbstractValidator<ToDoListDto>
{
    public CommandEditValidator()
    {
        RuleFor(x => x.Description).NotEmpty().WithMessage("Deskripsi tidak boleh kosong");
    }
}