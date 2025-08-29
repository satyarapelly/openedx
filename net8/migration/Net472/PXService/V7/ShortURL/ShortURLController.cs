// <copyright file="ShortURLController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using Microsoft.Commerce.Payments.PXService.Model.ShortURLDB;
    using OpenTelemetry.Trace;

    public class ShortURLController : ProxyController
    {
        [HttpPost]
        public async Task<HttpResponseMessage> Create([FromBody] CreateRequest request)
        {
            CreateResponse response = await this.Settings.ShortURLDBAccessor.Create(request);
            return Request.CreateResponse(HttpStatusCode.Created, response);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> Get(string code)
        {
            if (code == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Code is null.");
            }

            if (!Regex.IsMatch(code, "^[A-Z0-9]*$") || !(code.Length == PXCommon.Constants.ShortURL.CodeLength))
            {   
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            CodeEntry entry;
            try
            {
                entry = await this.Settings.ShortURLDBAccessor.GetCodeEntryAsync(code);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            if (entry == null || entry.MarkedForDeletion || DateTime.Now > entry.ExpireTime)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            entry.Hits++;
            await this.Settings.ShortURLDBAccessor.UpdateCodeEntryAsync(entry);

            string url = entry.URL;
            return Request.CreateResponse(HttpStatusCode.Redirect, url);
        }
    }
}