using System;
using Autobarn.Data;
using Autobarn.Data.Entities;
using Autobarn.Website.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Autobarn.Messages;
using EasyNetQ;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Autobarn.Website.Controllers.api {
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase {
        private readonly IAutobarnDatabase db;

        public VehiclesController(IAutobarnDatabase db) {
            this.db = db;
        }

        // GET: api/vehicles
        [HttpGet]
        [Produces("application/hal+json")]
        public IActionResult Get(int index = 0, int count = 10)
        {

            var a = Request.Headers["Authorization"];


            var items = db.ListVehicles().Skip(index).Take(count);
            var total = db.CountVehicles();
            dynamic _links = HypermediaExtensions.Paginate("/api/vehicles", index, count, total);
            dynamic _actions = new {
                create = new {
                    name = "Create",
                    type = "application/json",
                    method = "POST",
                    href = "/api/vehicles"
                }
            };
            var result = new {
                _links,
                _actions,
                total,
                index,
                count,
                items = items.Select(item => item.ToHal())
            };
            return Ok(result);
        }

        // GET api/vehicles/ABC123
        [HttpGet("{id}")]
        public IActionResult Get(string id) {
            var vehicle = db.FindVehicle(id);
            if (vehicle == default) return NotFound();
            var result = vehicle.ToHal();
            return Ok(result);
        }

        // POST api/vehicles
        [HttpPost]
        public IActionResult Post([FromBody] VehicleDto dto) {
            var vehicleModel = db.FindModel(dto.ModelCode);
            var vehicle = new Vehicle {
                Registration = dto.Registration,
                Color = dto.Color,
                Year = dto.Year,
                VehicleModel = vehicleModel
            };
            db.CreateVehicle(vehicle);
            PublishNewVehicleMessage(vehicle);
            var result = vehicle.ToHal();
            return Created($"{result._links.self}", result);
        }

        private void PublishNewVehicleMessage(Vehicle vehicle) {
            var m = new NewVehicleMessage() {
                Color = vehicle.Color,
                Registration = vehicle.Registration,
                Manufacturer = vehicle.VehicleModel?.Manufacturer?.Name,
                ModelName = vehicle.VehicleModel?.Name,
                ModelCode = vehicle.VehicleModel?.Code,
                Year = vehicle.Year,
                ListedAtUtc = DateTime.UtcNow
            };
        }

        // PUT api/vehicles/ABC123
        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] VehicleDto dto) {
            var vehicleModel = db.FindModel(dto.ModelCode);
            var vehicle = new Vehicle {
                Registration = dto.Registration,
                Color = dto.Color,
                Year = dto.Year,
                ModelCode = vehicleModel.Code
            };
            db.UpdateVehicle(vehicle);
            return Ok(dto);
        }

        // DELETE api/vehicles/ABC123
        [HttpDelete("{id}")]
        public IActionResult Delete(string id) {
            var vehicle = db.FindVehicle(id);
            if (vehicle == default) return NotFound();
            db.DeleteVehicle(vehicle);
            return NoContent();
        }
    }
}
