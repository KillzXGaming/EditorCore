using ByamlExt.Byaml;

var byaml = ByamlFile.LoadN("indoor.bcett.byml");
byaml.RootNode.Add("longboi", long.MaxValue);
File.WriteAllBytes("new.byaml", ByamlFile.SaveN(byaml));