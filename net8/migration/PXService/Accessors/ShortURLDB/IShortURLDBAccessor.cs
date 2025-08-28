// <copyright file="IShortURLDBAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.ShortURLDB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PXService.Model.ShortURLDB;
    using Microsoft.Commerce.Payments.PXService.Model.ShortURLService;
    using Microsoft.Commerce.Tracing;

    public interface IShortURLDBAccessor
    {
        Task<bool> CheckAndAddCodeEntryAsync(CodeEntry codeEntry);

        Task<CodeEntry> GetCodeEntryAsync(string code);

        Task UpdateCodeEntryAsync(CodeEntry codeEntry);

        Task<CreateResponse> Create(CreateRequest request);
    }
}
