using backend.DTOs.Responses;

namespace backend.Models.HelperModels;

public record DatabaseOutput
(
    bool IsSuccess,
    BaseResponse Response
);