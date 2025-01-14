using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Otterkit.Types;

public sealed class LocalEntries<TValue> where TValue: notnull
{
    private readonly Dictionary<string, List<TValue>> EntryLookup = new(StringComparer.OrdinalIgnoreCase);

    public void AddEntry(string entryName, TValue localEntry)
    {
        ref var entries = ref CollectionsMarshal.GetValueRefOrAddDefault(EntryLookup, entryName, out var exists);

        if (!exists)
        {
            entries = new(1);
            entries.Add(localEntry);
        }

        if (exists && entries is not null) entries.Add(localEntry);

        if (exists && entries is null)
        {
            throw new ArgumentException("Local entry exists but value was null in the EntryLookup dictionary", nameof(entryName));
        }
    }

    public bool EntryExists(string entryName)
    {
        ref var entries = ref CollectionsMarshal.GetValueRefOrNullRef(EntryLookup, entryName);

        if (!Unsafe.IsNullRef(ref entries)) return true;

        return false;
    }

    public (bool, bool) EntryExistsAndIsUnique(string entryName)
    {
        ref var entries = ref CollectionsMarshal.GetValueRefOrNullRef(EntryLookup, entryName);

        if (!Unsafe.IsNullRef(ref entries) && entries is not null)
        {
            return (true, entries.Count == 1);
        }

        return (false, false);
    }

    public List<TValue> GetEntriesByName(string entryName)
    {
        ref var entries = ref CollectionsMarshal.GetValueRefOrNullRef(EntryLookup, entryName);

        if (!Unsafe.IsNullRef(ref entries) && entries is not null)
        {
            return entries;
        }

        throw new ArgumentOutOfRangeException(nameof(entryName), "Local entry does not exist in the EntryLookup dictionary");
    }

    public TValue GetUniqueEntryByName(string entryName)
    {
        ref var entries = ref CollectionsMarshal.GetValueRefOrNullRef(EntryLookup, entryName);

        if (!Unsafe.IsNullRef(ref entries) && entries is not null)
        {
            return entries[0];
        }

        throw new ArgumentOutOfRangeException(nameof(entryName), "Local entry does not exist in the EntryLookup dictionary");
    }
}
