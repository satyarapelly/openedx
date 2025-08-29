// <copyright file="ShortURLController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Commerce.Payments.PXService.Model.ShortURLDB;

    public class ShortURLController : ProxyController
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRequest request)
        {
            var response = await this.Settings.ShortURLDBAccessor.Create(request);
            return this.StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpGet]
        public async Task<IActionResult> Get(string code)
        {
            if (code == null)
            {
                return this.BadRequest("Code is null.");
            }

            if (!Regex.IsMatch(code, "^[A-Z0-9]*$") || code.Length != PXCommon.Constants.ShortURL.CodeLength)
            {
                return this.NotFound();
            }

            CodeEntry entry;
            try
            {
                entry = await this.Settings.ShortURLDBAccessor.GetCodeEntryAsync(code);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

            if (entry == null || entry.MarkedForDeletion || DateTime.Now > entry.ExpireTime)
            {
                return this.NotFound();
            }

            entry.Hits++;
            await this.Settings.ShortURLDBAccessor.UpdateCodeEntryAsync(entry);

            string url = entry.URL;
            return this.Redirect(url);
        }
    }
}