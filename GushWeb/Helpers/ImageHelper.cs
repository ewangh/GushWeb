using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace GushWeb.Helpers
{
    //@using GushWeb.Helpers
    public static class ImageHelper
    {

        //@Html.Img
        public static MvcHtmlString Imgage(this HtmlHelper helper, string id, string url, string alternateText)
        {
            return Image(helper, id, url, alternateText, null);
        }

        public static MvcHtmlString Image(this HtmlHelper helper, string id, string url, string alternateText,object htmlAttributes)
        {
            var builder = new TagBuilder("img");
            builder.GenerateId(id);
            builder.MergeAttribute("src",url);
            builder.MergeAttribute("alt",alternateText);
            builder.MergeAttributes(new RouteValueDictionary(htmlAttributes));

            return MvcHtmlString.Create(builder.ToString(TagRenderMode.SelfClosing));
        }

        //@Html.ImageLink
        public static MvcHtmlString ImageLink(this HtmlHelper helper, string actionName, string imageUrl, string alternateText, object routValues, object linkHtmlAttributes, object imageHtmlAttributes)
        {
            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext);
            var url = urlHelper.Action(actionName,routValues);
            //建立链接
            var linkTagBuilder = new TagBuilder("a");
            linkTagBuilder.MergeAttribute("href",url);
            linkTagBuilder.MergeAttributes(new RouteValueDictionary(linkHtmlAttributes));
            //建立图片
            var imageTagBuilder = new TagBuilder("img");
            imageTagBuilder.MergeAttribute("src",urlHelper.Content(imageUrl));
            imageTagBuilder.MergeAttribute("alt",alternateText);
            imageTagBuilder.MergeAttribute("title",alternateText);
            imageTagBuilder.MergeAttributes(new RouteValueDictionary(imageHtmlAttributes));
            //将图片加至链接中
            linkTagBuilder.InnerHtml = imageTagBuilder.ToString(TagRenderMode.SelfClosing);

            return MvcHtmlString.Create(linkTagBuilder.ToString());
        }
    }
}