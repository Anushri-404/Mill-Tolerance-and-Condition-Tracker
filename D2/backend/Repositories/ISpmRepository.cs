using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models;

namespace backend.Repositories
{
    public interface ISpmRepository
    {
        Task<IEnumerable<string>> GetSectionsAsync();
        Task<IEnumerable<string>> GetEquipL1Async(string section);
        Task<IEnumerable<SpmEquipL2Dto>> GetEquipL2Async(string section, string equipL1);
        Task<SpmGreyPartsDto?> GetGreyPartsAsync(string equipL2Id);
        Task<IEnumerable<SpmCodeDto>> GetObservationTypesAsync();
        Task<IEnumerable<SpmCodeDto>> GetAffectedPortionsAsync();
        Task<bool> SaveObservationAsync(SpmObservationInput input, string? attachmentName, string? fileExtension);
        Task<IEnumerable<SpmObservationReportDto>> GetObservationReportAsync(SpmReportFilter filter);
    }
}