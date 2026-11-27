namespace SiloAI.Application.Shared;

/// <summary>The request is malformed or breaks a rule (HTTP 400). Thrown by handlers instead of controllers returning BadRequest.</summary>
public class BadRequestException(string message) : Exception(message);

/// <summary>The caller is authenticated but not allowed to do this (HTTP 403).</summary>
public class ForbiddenException(string message) : Exception(message);
