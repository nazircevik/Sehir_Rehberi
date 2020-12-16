using AutoMapper;
using SehirRehberi.API.Dtos;
using SehirRehberi.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SehirRehberi.API.Helpers
{
    public class AutoMapperProfiless:Profile
    {
        public AutoMapperProfiless()
        {
            CreateMap<City, CityForListDuo>().ForMember( dest=>dest.PhotoUrl,opt=>
            {
                opt.MapFrom(src => src.Photos.FirstOrDefault(P => P.IsMain).Url);
            });

            CreateMap<City, CityForDetailDto>();
            CreateMap<Photo, PhotoForCreateionDto>();
            CreateMap<PhotoForCreateionDto, Photo>();
        }
    }
}
