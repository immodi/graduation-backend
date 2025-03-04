using backend.DTOs.Responses;

namespace backend.Models;

public record DatabaseOutput
(
    bool IsSuccess,
    BaseResponse Response
);