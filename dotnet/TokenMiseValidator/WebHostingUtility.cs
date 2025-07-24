namespace PXCommon
{
    /// <summary>
    /// Minimal helper used by the sample to determine if the application is
    /// running in a self-hosted context. The default implementation simply
    /// returns <c>false</c>.
    /// </summary>
    public static class WebHostingUtility
    {
        public static bool IsApplicationSelfHosted() => false;
    }
}
