using Application.Core;
using Microsoft.EntityFrameworkCore;
using Persistence;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;
using Application.Interface;
using Infrastructure.Security;
using Infrastructure.PathFile;
using Infrastructure.Validation_Submission;
using Application.Interface.User;
using Infrastructure.User;
using Application.User.Students;
using Application.Learn.Courses.Query;
using Application.Learn.Courses.Command;
using Application.Learn.Lessons.Query;
using Application.Learn.Lessons.Command;
using Application.Assignments.Query;
using Application.Assignments.Command;
using Application.Submission.Query;
using Application.Submission.Command;
using Application.Attendances.Query;
using Application.Attendances.Command;
using Application.Learn.Schedules.Query;
using Application.Learn.Schedules.Command;
using Application.ClassRooms.Query;
using Application.ClassRooms.Command;

namespace API.Extensions
{
    // Ketika kita membuat metode Ekstensi, maka kita perlu memastikan class kita adalah static
    public static class ApplicationServiceExtensions
    {
        // Dan kemudian kita membuat metode  ekstensi itu sendiri, dan itu akan disebut public
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        /* Sekarang parameter pertama dari metode ekstensi ini adalah hal yang akan kita perluas daftar*/
        {
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Emboiii J.A.", Version = "v1" });

                    // Define BearerAuth scheme
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme",
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer"
                    });

                    // Add BearerAuth as requirement for operations
                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}
                        }
                    });
                });

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddDbContext<DataContext>(opt =>
            {
                opt.UseNpgsql(config.GetConnectionString("KoneksiKePostgreSQL"));
            });

            // Menambahkan CORS Policy
            services.AddCors(opt =>
            {
                opt.AddPolicy("CorsPolicy", policy =>
                {
                    policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:5173");
                });
            });

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ListStudent.Handler).Assembly));

            /** ASSIGNMENT Query **/
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DetailsAssignment.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DownloadAssignment.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ListAssignmentsByClassRoomId.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ListAssignmentsByTeacherId.Handler).Assembly));
            /** ASSIGNMENT Command **/
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateAssignment.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EditAssignment.Handler).Assembly));

            /** ASSIGNMENTSUBMISSION Query **/
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetListSubmissionForTeacherGrades.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetSubmissionForStudentByAssignmentId.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetSubmissionForTeacherBySubmissionId.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DownloadSubmission.Handler).Assembly));
            /** ASSIGNMENTSUBMISSION Command **/
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateSubmissionByStudentId.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EditSubmissionByTeacherId.Handler).Assembly));

            /** ATTENDANCE Query **/
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ListAttendance.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DetailsAttendance.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAllByStudentId.Handler).Assembly));
            /** ATTENDANCE Command **/
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateAttendance.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EditAttendance.Handler).Assembly));

            /** CLASSROOM Query **/
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ListClassRoom.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DetailsClassRoom.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ClassRoomTeacher.Handler).Assembly));
            /** CLASSROOM Command **/
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateClassRoom.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EditClassRoom.Handler).Assembly));

            /** COURSE Query **/
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DetailsCourse.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ListCourseByClassRoomId.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ListCourseByTeacherId.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DownloadCourse.Handler).Assembly));
            /** COURSE Command **/
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateCourse.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EditCourse.Handler).Assembly));

            /** LESSON Query **/
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ListLesson.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DetailsLesson.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LessonByClassRoomId.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LessonByTeacherId.Handler).Assembly));
            /** LESSON Command **/
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateLesson.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EditLesson.Handler).Assembly));

            /** SCHEDULE Query **/
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ListSchedule.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ListScheduleById.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ListSchedulesByClassRoomId.Handler).Assembly));
            /** SCHEDULE Command **/
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateSchedule.Handler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EditSchedule.Handler).Assembly));

            services.AddAutoMapper(typeof(MappingProfiles).Assembly);
            services.AddFluentValidationAutoValidation();

            /** ASSIGNMENT Validation **/
            services.AddValidatorsFromAssemblyContaining<CreateAssignment>();
            services.AddValidatorsFromAssemblyContaining<DeactivateAssignment>();
            services.AddValidatorsFromAssemblyContaining<EditAssignment>();

            /** ASSIGNMENTSUBMISSION Validation **/
            services.AddValidatorsFromAssemblyContaining<CreateSubmissionByStudentId>();
            services.AddValidatorsFromAssemblyContaining<EditSubmissionByTeacherId>();

            /** ATTENDANCE Validation **/
            services.AddValidatorsFromAssemblyContaining<CreateAttendance>();
            services.AddValidatorsFromAssemblyContaining<EditAttendance>();

            /** CLASSROOM Validation **/
            services.AddValidatorsFromAssemblyContaining<CreateClassRoom>();
            services.AddValidatorsFromAssemblyContaining<EditClassRoom>();

            /** COURSE Validation **/
            services.AddValidatorsFromAssemblyContaining<CreateCourse>();
            services.AddValidatorsFromAssemblyContaining<DeactivateCourse>();
            services.AddValidatorsFromAssemblyContaining<EditCourse>();

            /** LESSON Validation **/
            services.AddValidatorsFromAssemblyContaining<CreateLesson>();
            services.AddValidatorsFromAssemblyContaining<DeactivateLesson>();
            services.AddValidatorsFromAssemblyContaining<EditLesson>();

            /** SCHEDULE Validation **/
            services.AddValidatorsFromAssemblyContaining<CreateSchedule>();
            services.AddValidatorsFromAssemblyContaining<EditSchedule>();

            services.AddValidatorsFromAssemblyContaining<CreateStudentWithExcel>();

            services.AddValidatorsFromAssemblyContaining<CreateStudent>();
            services.AddHttpContextAccessor();

            /** INTERFACE **/
            services.AddScoped<IUserAccessor, UserAccessor>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IUserTeacher, UserTeacher>();
            services.AddScoped<ISubmissionService, SubmissionService>();

            return services;
        }
    }
}