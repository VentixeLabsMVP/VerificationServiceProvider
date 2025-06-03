﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VerificationController(IVerificationService verificationService) : ControllerBase
{
    private readonly IVerificationService _verificationService = verificationService;


    [HttpPost("send")]
    public async Task<IActionResult> Send(SendVerificationCodeRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { Error = "Recipient email address is required." });

        var result = await _verificationService.SendVerificationCodeAsync(request);
        return result.Suceeded
            ? Ok(result)
            : StatusCode(500, result);
    }
   
    [HttpPost("verify")]
    public IActionResult Verify(VerifyVerificationCodeRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { Error = "Invalid or expired verification code." });

        var result = _verificationService.VerifyVerificationCOde(request);
        return result.Suceeded
            ? Ok(result)
            : StatusCode(500, result);
    }
}
