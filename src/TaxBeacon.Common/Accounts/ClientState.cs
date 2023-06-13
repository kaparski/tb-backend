﻿using Ardalis.SmartEnum;

namespace TaxBeacon.Common.Accounts;

public class ClientState: SmartEnum<ClientState>
{
    public static readonly ClientState None = new("None", 0);
    public static readonly ClientState ClientProspect = new("Client Prospect", 1);
    public static readonly ClientState Client = new("Client", 2);
    public static readonly ClientState ClientReferral = new("Client Referral", 3);

    private ClientState(string name, int value) : base(name, value)
    {
    }
}
