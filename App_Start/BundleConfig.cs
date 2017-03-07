using System.Web;
using System.Web.Optimization;

namespace OPAS2
{
  public class BundleConfig
  {
    // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
    public static void RegisterBundles(BundleCollection bundles)
    {
      bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                  "~/Scripts/jquery-{version}.js"));

      bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                  "~/Scripts/jquery.validate*"));

      // Use the development version of Modernizr to develop with and learn from. Then, when you're
      // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
      bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                  "~/Scripts/modernizr-*"));

      bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/respond.js"));

      bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/vue-element-ui.css",
                "~/Content/site.css"));

      #region Customized bundles
      bundles.Add(new ScriptBundle("~/bundles/vue").Include(
                  "~/Scripts/vue-{version}.js"));
      bundles.Add(new ScriptBundle("~/bundles/vue-element-ui").Include(
            "~/Scripts/vue-element-ui.js"));
      bundles.Add(new ScriptBundle("~/bundles/lodash").Include(
            "~/Scripts/lodash.js"));
      bundles.Add(new ScriptBundle("~/bundles/guid").Include(
            "~/Scripts/guid.js"));
      bundles.Add(new ScriptBundle("~/bundles/webtoolkitbase64").Include(
            "~/Scripts/webtoolkitbase64.js"));
      bundles.Add(new ScriptBundle("~/bundles/axios").Include(
            "~/Scripts/axios.js"));
      bundles.Add(new ScriptBundle("~/bundles/currency-validator").Include(
            "~/Scripts/currency-validator.js"));
      bundles.Add(new ScriptBundle("~/bundles/my-vue-components").Include(
            "~/Scripts/my-vue-components.js"));
      bundles.Add(new ScriptBundle("~/bundles/opas-vue-biz-document-mixin").Include(
      "~/Scripts/opas-vue-biz-document-mixin.js"));
      #endregion
    }
  }
}
