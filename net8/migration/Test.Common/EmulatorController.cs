// <copyright file="EmulatorController.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2017. All rights reserved.</copyright>
namespace Test.Common
{
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Commerce.Payments.Common.Transaction;
	using Microsoft.Commerce.Payments.Common.Web;
	using Test.Common.Extensions; 
	using System.Text;
	using System.Threading.Tasks;

    public class EmulatorController : ControllerBase
    {
        private readonly TestScenarioManager _testScenarioManager;

        public EmulatorController(TestScenarioManager testScenarioManager)
        {
            _testScenarioManager = testScenarioManager;
        }

        protected async Task<IActionResult> GetResponseAsync(string apiName)
        {
            if (!HttpContext.TryGetTestContext(out var testContext))
            {
                return BadRequest("TestContext not found.");
            }

            var result = _testScenarioManager.GetResponseContent(apiName, testContext);
            if (result == null)
            {
                return NotFound();
            }

            HttpContext.Response.StatusCode = result.StatusCode;
            HttpContext.Response.ContentType = result.ContentType ?? "application/json";

            if (!string.IsNullOrEmpty(result.Content))
            {
                await HttpContext.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(result.Content));
            }

            return new EmptyResult(); // Response already written to body
        }
    }
}
