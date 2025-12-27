using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Shared.Responses;

namespace WebAPI.ApiConventions;



public static class AppResponseApiConventions
{
    // ========== GET ==========
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    [ProducesResponseType(typeof(AppResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponse), StatusCodes.Status500InternalServerError)]
    public static void Get(
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
        params object[] _)
    { }

    // ========== POST ==========
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    [ProducesResponseType(typeof(AppResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponse), StatusCodes.Status500InternalServerError)]
    public static void Post(
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
        params object[] _)
    { }

    // ========== PUT ==========
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    [ProducesResponseType(typeof(AppResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponse), StatusCodes.Status500InternalServerError)]
    public static void Put(
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
        params object[] _)
    { }

    // ========== PATCH ==========
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    [ProducesResponseType(typeof(AppResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponse), StatusCodes.Status500InternalServerError)]
    public static void Patch(
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
        params object[] _)
    { }

    // ========== DELETE (لو احتجته لاحقًا) ==========
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    [ProducesResponseType(typeof(AppResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponse), StatusCodes.Status500InternalServerError)]
    public static void Delete(
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
        params object[] _)
    { }
}