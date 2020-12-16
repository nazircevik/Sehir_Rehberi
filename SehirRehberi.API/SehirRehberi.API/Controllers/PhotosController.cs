﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SehirRehberi.API.Data;
using SehirRehberi.API.Dtos;
using SehirRehberi.API.Helpers;
using SehirRehberi.API.Models;

namespace SehirRehberi.API.Controllers
{
    [Route("api/[controller]/{cityId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private IAppRepository _appRepository;
        private IMapper _mapper;
        private IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;
        public PhotosController(IAppRepository appRepository, IMapper mapper, IOptions<CloudinarySettings> options)
        {
            _mapper = mapper;
            _cloudinaryConfig = options;
            _appRepository = appRepository;

            Account account = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
                );
            _cloudinary = new Cloudinary(account);
        }
        [HttpPost]
        public ActionResult AddPhotoForCity(int cityId, [FromBody] PhotoForCreateionDto photoForCreateionDto)
        {
            var city = _appRepository.GetCityById(cityId);
            //datatemper engellemek için
            if(city==null)
            {
                return BadRequest("Could not find the city");
            }
            var currentUserId= int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if(currentUserId!=city.UserId)
            {
                return Unauthorized();
            }
            var file = photoForCreateionDto.File;
            var uploadResult = new ImageUploadResult();
            if(file.Length>0)
            {
                using (var stream=file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File=new FileDescription(file.Name,stream)
                    };
                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }
            photoForCreateionDto.Url = uploadResult.Url.ToString();
            photoForCreateionDto.PublicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(photoForCreateionDto);
            photo.City = city;

            if(!city.Photos.Any(p=>p.IsMain))
            {
                photo.IsMain = true;
            }
            city.Photos.Add(photo);
            if(_appRepository.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
                return CreatedAtRoute("GetPhoto",new { id=photo.Id},photoToReturn);
            }
            return BadRequest("Could not add the photo");
        }
        [HttpGet("{id}",Name ="GetPhoto")]
        public ActionResult GetPhoto(int id)
        {
            var photoFromDb = _appRepository.GetPhotosByCity(id);
            var photo = _mapper.Map<PhotoForCreateionDto>(photoFromDb);
            return Ok(photo);

        }
    }
}
