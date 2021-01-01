using Api.Hosting.Dto;
using Api.Service.Exceptions;
using Api.Service.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Linq;

namespace Api.Hosting.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/recipes")]
    [Produces("application/json")]
    [Authorize(Constants.Policies.Admin)]
    public class AdminRecipesController : Controller
    {
        private ILogger _logger;
        private RecipesService _recipesService;
        private QuantityUnitsService _qtyService;

        public AdminRecipesController(ILogger<AdminRecipesController> logger, RecipesService recipesService, QuantityUnitsService qtyService)
        {
            _logger = logger;
            _recipesService = recipesService;
            _qtyService = qtyService;
        }

        #region Quantity units

        [HttpGet("quantity-units")]
        [SwaggerOperation("Retrieve the full list of quantity units available")]
        [SwaggerResponse(200, "The full list", typeof(QuantityUnitDto[]))]
        [SwaggerResponse(500, "Unexpected error")]
        public IActionResult GetAll()
        {
            try
            {
                var list = _qtyService.GetAll();

                return Ok(list.Select(e => e.Dto()).ToArray());
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot retrieve the list of Quantity units - {e}");
                return StatusCode(500, "An unexpected error occured retrieving the units");
            }
        }

        [HttpPost("quantity-units")]
        [SwaggerOperation("Creates a new quantity unit")]
        [SwaggerResponse(201, "The unit created", typeof(QuantityUnitDto))]
        [SwaggerResponse(500, "An error occured")]
        public IActionResult CreateQuantityUnit([FromBody] QuantityUnitDto qtyUnit)
        {
            try
            {
                var saved = _qtyService.Create(qtyUnit.Model());

                return Ok(saved.Dto());
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot save the new unit, an unexpected error occured - {e}");
                return StatusCode(500, "Cannot create a new quantity unit");
            }
        }

        [HttpPut("quantity-units/{id}")]
        [SwaggerOperation("Updates an existing quantity unit")]
        [SwaggerResponse(200, "Quantity unit updated", typeof(QuantityUnitDto))]
        [SwaggerResponse(404, "Quantity unit does not exist")]
        [SwaggerResponse(500, "An error occured")]
        public IActionResult UpdateQuantityUnit([FromRoute] string id, [FromBody] QuantityUnitDto qtyUnit)
        {
            try
            {
                var oldUnit = _qtyService.GetOne(id);
                if (oldUnit == null)
                {
                    return NotFound();
                }
                var updatedModel = qtyUnit.Model();
                updatedModel.Id = oldUnit.Id;
                var result = _qtyService.Update(updatedModel);
                if (!result) throw new Exception("Mongo cannot save quantity unit");
                var updatedUnit = _qtyService.GetOne(id);

                return Ok(updatedUnit.Dto());
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot save the unit, an unexpected error occured - {e}");
                return StatusCode(500, "Cannot update the quantity unit");
            }
        }

        [HttpDelete("quantity-units/{id}")]
        [SwaggerOperation("Removes an quantity unit")]
        [SwaggerResponse(200, "Quantity unit removed", typeof(string))]
        [SwaggerResponse(404, "Quantity unit does not exist")]
        [SwaggerResponse(500, "An error occured")]
        public IActionResult RemoveQuantityUnit([FromRoute] string id)
        {
            try
            {
                var result = _qtyService.Remove(id);
                return Ok(id);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot remove quantity unit {id} - {e}");
                return StatusCode(500, "Server error while deleting quantity unit");
            }
        }

        #endregion
    }
}
