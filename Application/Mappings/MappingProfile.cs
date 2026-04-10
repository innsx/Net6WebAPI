using Application.CQRSs.Products.Commands;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class MappingProfile : Profile
    {
        //HERE IS WHERE MAPPINGS IS CONFIGURED
        public MappingProfile()
        {
            //Mapping from Obj1 to obj2 in its enteriely
            //CreateMap<TSource, TDestination>
            //CreateMap<CreateProductCommand, Product>();

            //CreateMap<TSource, TDestination> with REVERSE MAPPING
            //CreateMap<CreateProductCommand, Product>().ReverseMap();

            ////Mapping from Obj1 to obj2 in its enteriely with a SPECIFIC PROPERTY MAPPING
            //CreateMap<CreateProductCommand, Product>()
            //    .ForMember(dest=> dest.Description, 
            //    src => src.MapFrom(
            //        src => src.Remarks));

            //Mapping from Obj1 to obj2 in its enteriely with a SPECIFIC PROPERTY MAPPING
            //with REVERSE MAPPING from obj2 to obj1
            CreateMap<CreateProductCommand, Product>()
                .ForMember(dest => dest.Description, src => src.MapFrom(src => src.Remarks))
                .ReverseMap();
        }
    }
}
