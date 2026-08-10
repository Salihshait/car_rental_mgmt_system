using System.Security.Claims;
using CarRent.Application.DTOs.Finance;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/finance/bank-accounts")]
[Authorize(Roles = AdminRoles)]
public class BankAccountsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IBankAccountService _bankAccountService;

    public BankAccountsController(IBankAccountService bankAccountService)
    {
        _bankAccountService = bankAccountService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await _bankAccountService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await _bankAccountService.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBankAccountRequest request, CancellationToken cancellationToken) =>
        Ok(await _bankAccountService.CreateAsync(request, cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBankAccountRequest request, CancellationToken cancellationToken) =>
        Ok(await _bankAccountService.UpdateAsync(id, request, cancellationToken));

    [HttpGet("{id:guid}/transactions")]
    public async Task<IActionResult> GetTransactions(Guid id, CancellationToken cancellationToken) =>
        Ok(await _bankAccountService.GetTransactionsAsync(id, cancellationToken));

    [HttpPost("{id:guid}/transactions")]
    public async Task<IActionResult> AddTransaction(Guid id, [FromBody] CreateBankTransactionRequest request, CancellationToken cancellationToken) =>
        Ok(await _bankAccountService.AddTransactionAsync(id, CurrentUserId, request, cancellationToken));
}
