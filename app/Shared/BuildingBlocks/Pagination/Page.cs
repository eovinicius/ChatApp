namespace BuildingBlocks.Pagination;

// Uma fatia de uma lista com paginação keyset. NextCursor é o valor de ordenação
// do último item devolvido — o cliente o manda de volta como "before".
public sealed record Page<TItem>(IReadOnlyList<TItem> Items, DateTime? NextCursor, bool HasMore)
{
    public static Page<TItem> Empty() => new([], null, false);

    // Deixa o handler recortar a página sobre o read model cru e só depois
    // projetar para o DTO — o cursor sai da linha do banco, não do DTO.
    public Page<TOther> Map<TOther>(Func<TItem, TOther> map) =>
        new(Items.Select(map).ToList(), NextCursor, HasMore);
}

public static class Page
{
    // O handler pede take+1 ao DAO; o item extra é a prova de que existe próxima
    // página, sem precisar de um COUNT(*) separado. Ele é descartado aqui.
    public static Page<TItem> From<TItem>(IReadOnlyList<TItem> fetched, int take, Func<TItem, DateTime> cursor)
    {
        var hasMore = fetched.Count > take;

        var items = hasMore
            ? fetched.Take(take).ToList()
            : fetched;

        return new Page<TItem>(
            items,
            items.Count == 0 ? null : cursor(items[^1]),
            hasMore);
    }
}
