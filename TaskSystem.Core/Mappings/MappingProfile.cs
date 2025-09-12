using AutoMapper;
using TaskSystem.Core.DTOs;
using TaskSystem.Core.Entities;

namespace TaskSystem.Core.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.Ignore()); // Will be mapped manually with Identity roles
        CreateMap<CreateUserRequest, User>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));
        CreateMap<UpdateUserRequest, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Task mappings
        CreateMap<TaskItem, TaskDto>()
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedBy.FullName))
            .ForMember(dest => dest.AssignedToName, opt => opt.MapFrom(src => src.AssignedTo != null ? src.AssignedTo.FullName : null));
        CreateMap<CreateTaskRequest, TaskItem>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => TaskItemStatus.Pending));
        CreateMap<UpdateTaskRequest, TaskItem>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Auth mappings
        CreateMap<RegisterRequest, User>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));
    }
}