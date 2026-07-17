namespace Domu.Api.Features.ShoppingLists.Application.Items.Contracts;

public sealed record InventorySubmissionResult(IReadOnlyList<InventorySubmissionOutcome> Outcomes);
public sealed record InventorySubmissionOutcome(Guid ShoppingListItemId, Guid? CreatedEntryId, string? SkippedReason);
