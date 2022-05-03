﻿using Kncs.CrdController.Crd;

namespace Kncs.CrdController.CSharpApp;

public class CSharpAppSpec
{
    public string? Code { get; set; }
    public byte Replicas { get; set; }
    public CSharpAppService? Service { get; set; }
}

public class CSharpAppStatus
{
    public bool Created { get; set; }
}

public class CSharpAppService
{
    public short Port { get; set; }
    public string? Type { get; set; }

    public override bool Equals(object? obj)
    {
        var item = obj as CSharpAppService;

        if (item == null)
        {
            return false;
        }

        return this.Port == item.Port && (bool)(this.Type?.Equals(item.Type));
    }

    // ReSharper disable NonReadonlyMemberInGetHashCode
    public override int GetHashCode()
    {
        if (this.Type == null)
        {
            return this.Port.GetHashCode() ^ "null".GetHashCode();
        }
        
        
        return this.Port.GetHashCode() ^ Type.GetHashCode();
    }
}

public class LastApplyConfiguration
{
    // ReSharper disable once InconsistentNaming
    public int CodeHash { get; set; }
    public byte Replicas { get; set; }
    public CSharpAppService? Service { get; set; } 
}

public class CSharpApp: CustomResource<CSharpAppSpec, CSharpAppStatus>
{
    
}