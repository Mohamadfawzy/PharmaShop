namespace Shared.Enums.Prescription;

public enum PrescriptionSortOption : byte
{
    Newest = 1,           // CreatedAt desc
    Oldest = 2,           // CreatedAt asc
    StatusUpdatedDesc = 3 // StatusUpdatedAt desc
}

