using System.Security.Claims;
using SiloAI.Application.Shared.Contracts.Customers;
using SiloAI.Application.Shared.Features;
using SiloAI.Domains;
using Xunit;

namespace SiloAI.Tests;

public class CustomerContextTests
{
    private static ClaimsPrincipal Principal(params Claim[] claims)
        => new(new ClaimsIdentity(claims, "test", ClaimTypes.Name, ClaimTypes.Role));

    [Fact]
    public void A_customer_token_yields_the_customer_id_and_a_customer_owner_key()
    {
        var caller = CallerIdentity.FromClaims(Principal(new Claim("CustomerId", "42"), new Claim(ClaimTypes.Name, "acme")));

        Assert.Equal(42, caller.CustomerId);
        Assert.Equal("customer:42", caller.OwnerId);
        Assert.Equal("acme", caller.Username);
    }

    [Fact]
    public void An_admin_token_has_no_customer_and_is_owned_by_the_user()
    {
        var caller = CallerIdentity.FromClaims(Principal(new Claim(ClaimTypes.NameIdentifier, "7"), new Claim(ClaimTypes.Name, "admin")));

        Assert.Null(caller.CustomerId);
        Assert.Equal("user:7", caller.OwnerId);
    }

    [Fact]
    public void Without_any_id_the_owner_falls_back_to_the_name()
    {
        var caller = CallerIdentity.FromClaims(Principal(new Claim(ClaimTypes.Name, "someone")));

        Assert.Equal("someone", caller.OwnerId);
        Assert.Null(caller.CustomerId);
    }

    [Fact]
    public void A_non_numeric_customer_claim_is_not_a_customer_id()
    {
        var caller = CallerIdentity.FromClaims(Principal(new Claim("CustomerId", "abc")));

        Assert.Null(caller.CustomerId);
    }

    [Fact]
    public void No_principal_is_anonymous()
    {
        Assert.Equal(CallerIdentity.Anonymous, CallerIdentity.FromClaims(null));
        Assert.Null(CallerIdentity.Anonymous.CustomerId);
    }

    private sealed class FixedCaller(int? customerId, string owner, string name) : ICustomerContext
    {
        public int? CustomerId => customerId;
        public string OwnerId => owner;
        public string Username => name;
    }

    [Fact]
    public void The_caller_overwrites_whatever_the_client_sent()
    {
        var command = new RagChatSendCommand { Message = "hi", CustomerId = 999, OwnerId = "customer:999", Username = "spoofed" };

        CallerStamp.Apply(command, new FixedCaller(5, "customer:5", "real"));

        Assert.Equal(5, command.CustomerId);
        Assert.Equal("customer:5", command.OwnerId);
        Assert.Equal("real", command.Username);
    }

    [Fact]
    public void An_admin_caller_clears_a_spoofed_customer_id()
    {
        var command = new OcrCommand { CustomerId = 999 };

        CallerStamp.Apply(command, new FixedCaller(null, "user:1", "admin"));

        Assert.Null(command.CustomerId);
    }

    [Fact]
    public void Requests_that_do_not_ask_for_the_caller_are_left_alone()
    {
        CallerStamp.Apply(new object(), new FixedCaller(5, "o", "n"));
        CallerStamp.Apply(new GetNegativeCreditCustomersQuery(), new FixedCaller(5, "o", "n"));
    }

    [Fact]
    public void Customers_stop_on_insufficient_credit_unless_an_admin_says_otherwise()
    {
        Assert.True(new Customer().StopOnInsufficientCredit);
        Assert.True(new CreateCustomerCommand().StopOnInsufficientCredit);
        Assert.True(new UpdateCustomerCommand().StopOnInsufficientCredit);
        Assert.True(new CustomerDto().StopOnInsufficientCredit);
    }
}
