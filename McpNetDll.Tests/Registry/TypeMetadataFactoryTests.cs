using System;
using System.Linq;
using dnlib.DotNet;
using McpNetDll;
using McpNetDll.Registry;
using Xunit;

namespace McpNetDll.Tests.Registry;

public class TypeMetadataFactoryTests
{
    [Fact]
    public void CreateTypeMetadata_ForClass_ReturnsCorrectTypeKind()
    {
        var module = new ModuleDefUser("TestModule");
        var typeDef = new TypeDefUser("TestNamespace", "TestClass", module.CorLibTypes.Object.TypeDefOrRef);
        typeDef.Attributes = TypeAttributes.Public | TypeAttributes.Class;
        module.Types.Add(typeDef);

        var metadata = TypeMetadataFactory.CreateTypeMetadata(typeDef);

        Assert.Equal("TestClass", metadata.Name);
        Assert.Equal("TestNamespace", metadata.Namespace);
        Assert.Equal("class", metadata.TypeKind);
    }

    [Fact]
    public void CreateTypeMetadata_ForInterface_ReturnsCorrectTypeKind()
    {
        var module = new ModuleDefUser("TestModule");
        var typeDef = new TypeDefUser("TestNamespace", "ITestInterface", null);
        typeDef.Attributes = TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract;
        module.Types.Add(typeDef);

        var metadata = TypeMetadataFactory.CreateTypeMetadata(typeDef);

        Assert.Equal("ITestInterface", metadata.Name);
        Assert.Equal("interface", metadata.TypeKind);
    }

    [Fact]
    public void CreateTypeMetadata_ForEnum_ReturnsCorrectTypeKindAndValues()
    {
        var module = new ModuleDefUser("TestModule");
        var enumType = new TypeDefUser("TestNamespace", "TestEnum", module.CorLibTypes.GetTypeRef("System", "Enum"));
        enumType.Attributes = TypeAttributes.Public | TypeAttributes.Sealed;
        module.Types.Add(enumType);

        var valueField = new FieldDefUser("__value__", new FieldSig(module.CorLibTypes.Int32), FieldAttributes.Public | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName);
        enumType.Fields.Add(valueField);

        var enumValue1 = new FieldDefUser("Value1", new FieldSig(new ValueTypeSig(enumType)), FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal);
        enumValue1.Constant = module.UpdateRowId(new ConstantUser(0, module.CorLibTypes.Int32.ElementType));
        enumType.Fields.Add(enumValue1);

        var enumValue2 = new FieldDefUser("Value2", new FieldSig(new ValueTypeSig(enumType)), FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal);
        enumValue2.Constant = module.UpdateRowId(new ConstantUser(1, module.CorLibTypes.Int32.ElementType));
        enumType.Fields.Add(enumValue2);

        var metadata = TypeMetadataFactory.CreateTypeMetadata(enumType);

        Assert.Equal("enum", metadata.TypeKind);
        Assert.NotNull(metadata.EnumValues);
        Assert.Equal(2, metadata.EnumValues.Count);
        Assert.Contains(metadata.EnumValues, ev => ev.Name == "Value1" && ev.Value == "0");
        Assert.Contains(metadata.EnumValues, ev => ev.Name == "Value2" && ev.Value == "1");
    }

    [Fact]
    public void CreateTypeMetadata_ForStaticClass_ReturnsCorrectTypeKind()
    {
        var module = new ModuleDefUser("TestModule");
        var typeDef = new TypeDefUser("TestNamespace", "StaticClass", module.CorLibTypes.Object.TypeDefOrRef);
        typeDef.Attributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed;
        module.Types.Add(typeDef);

        var metadata = TypeMetadataFactory.CreateTypeMetadata(typeDef);

        Assert.Equal("static class", metadata.TypeKind);
    }

    [Fact]
    public void CreateTypeMetadata_ForAbstractClass_ReturnsCorrectTypeKind()
    {
        var module = new ModuleDefUser("TestModule");
        var typeDef = new TypeDefUser("TestNamespace", "AbstractClass", module.CorLibTypes.Object.TypeDefOrRef);
        typeDef.Attributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Abstract;
        module.Types.Add(typeDef);

        var metadata = TypeMetadataFactory.CreateTypeMetadata(typeDef);

        Assert.Equal("abstract class", metadata.TypeKind);
    }

    [Fact]
    public void CreateTypeMetadata_ForSealedClass_ReturnsCorrectTypeKind()
    {
        var module = new ModuleDefUser("TestModule");
        var typeDef = new TypeDefUser("TestNamespace", "SealedClass", module.CorLibTypes.Object.TypeDefOrRef);
        typeDef.Attributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed;
        module.Types.Add(typeDef);

        var metadata = TypeMetadataFactory.CreateTypeMetadata(typeDef);

        Assert.Equal("sealed class", metadata.TypeKind);
    }

    [Fact]
    public void CreateTypeMetadata_ForStruct_ReturnsCorrectTypeKind()
    {
        var module = new ModuleDefUser("TestModule");
        var typeDef = new TypeDefUser("TestNamespace", "TestStruct", module.CorLibTypes.GetTypeRef("System", "ValueType"));
        typeDef.Attributes = TypeAttributes.Public | TypeAttributes.Sealed;
        module.Types.Add(typeDef);

        var metadata = TypeMetadataFactory.CreateTypeMetadata(typeDef);

        Assert.Equal("struct", metadata.TypeKind);
    }

    [Fact]
    public void CreateTypeMetadata_WithPublicMethods_CountsCorrectly()
    {
        var module = new ModuleDefUser("TestModule");
        var typeDef = new TypeDefUser("TestNamespace", "TestClass", module.CorLibTypes.Object.TypeDefOrRef);
        typeDef.Attributes = TypeAttributes.Public | TypeAttributes.Class;
        module.Types.Add(typeDef);

        var publicMethod = new MethodDefUser("PublicMethod", MethodSig.CreateInstance(module.CorLibTypes.Void), MethodAttributes.Public);
        typeDef.Methods.Add(publicMethod);

        var privateMethod = new MethodDefUser("PrivateMethod", MethodSig.CreateInstance(module.CorLibTypes.Void), MethodAttributes.Private);
        typeDef.Methods.Add(privateMethod);

        var metadata = TypeMetadataFactory.CreateTypeMetadata(typeDef);

        Assert.Equal(1, metadata.MethodCount);
        Assert.Single(metadata.Methods!);
        Assert.Equal("PublicMethod", metadata.Methods![0].Name);
    }

    [Fact]
    public void CreateTypeMetadata_WithProperties_IncludesPropertyMetadata()
    {
        var module = new ModuleDefUser("TestModule");
        var typeDef = new TypeDefUser("TestNamespace", "TestClass", module.CorLibTypes.Object.TypeDefOrRef);
        typeDef.Attributes = TypeAttributes.Public | TypeAttributes.Class;
        module.Types.Add(typeDef);

        var property = new PropertyDefUser("TestProperty", new PropertySig(true, module.CorLibTypes.String));
        var getter = new MethodDefUser("get_TestProperty", MethodSig.CreateInstance(module.CorLibTypes.String), MethodAttributes.Public | MethodAttributes.SpecialName);
        property.GetMethod = getter;
        typeDef.Methods.Add(getter);
        typeDef.Properties.Add(property);

        var metadata = TypeMetadataFactory.CreateTypeMetadata(typeDef);

        Assert.Equal(1, metadata.PropertyCount);
        Assert.Single(metadata.Properties!);
        Assert.Equal("TestProperty", metadata.Properties![0].Name);
        Assert.Equal("System.String", metadata.Properties![0].Type);
    }

    [Fact]
    public void CreateTypeMetadata_SpecialNameMethods_AreNotCounted()
    {
        var module = new ModuleDefUser("TestModule");
        var typeDef = new TypeDefUser("TestNamespace", "TestClass", module.CorLibTypes.Object.TypeDefOrRef);
        typeDef.Attributes = TypeAttributes.Public | TypeAttributes.Class;
        module.Types.Add(typeDef);

        var specialMethod = new MethodDefUser("get_Property", MethodSig.CreateInstance(module.CorLibTypes.String), MethodAttributes.Public | MethodAttributes.SpecialName);
        typeDef.Methods.Add(specialMethod);

        var normalMethod = new MethodDefUser("NormalMethod", MethodSig.CreateInstance(module.CorLibTypes.Void), MethodAttributes.Public);
        typeDef.Methods.Add(normalMethod);

        var metadata = TypeMetadataFactory.CreateTypeMetadata(typeDef);

        Assert.Equal(1, metadata.MethodCount);
        Assert.Single(metadata.Methods!);
        Assert.Equal("NormalMethod", metadata.Methods![0].Name);
    }
}