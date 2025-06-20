﻿using System.Collections.Generic;

namespace KsqlDsl.Core.Abstractions;

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
