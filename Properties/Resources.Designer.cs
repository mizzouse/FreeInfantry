// Decompiled with JetBrains decompiler
// Type: InfantryLauncher.Properties.Resources
// Assembly: InfantryLauncher, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BCEC389A-35DE-4913-97AA-CBDD8E941EC4
// Assembly location: C:\Program Files (x86)\Infantry Online\InfantryLauncher.exe

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace InfantryLauncher.Properties
{
  [DebuggerNonUserCode]
  [CompilerGenerated]
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
  internal class Resources
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if (object.ReferenceEquals((object) Resources.resourceMan, (object) null))
          Resources.resourceMan = new ResourceManager("InfantryLauncher.Properties.Resources", typeof (Resources).Assembly);
        return Resources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get
      {
        return Resources.resourceCulture;
      }
      set
      {
        Resources.resourceCulture = value;
      }
    }

    internal static Icon MainIcon
    {
      get
      {
        return (Icon) Resources.ResourceManager.GetObject("MainIcon", Resources.resourceCulture);
      }
    }

    internal Resources()
    {
    }
  }
}
