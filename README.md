# Enum As Index

<p align="center">
	<img alt="GitHub package.json version" src ="https://img.shields.io/github/package-json/v/Thundernerd/Unity3D-EnumAsIndex" />
	<a href="https://github.com/Thundernerd/Unity3D-EnumAsIndex/issues">
		<img alt="GitHub issues" src ="https://img.shields.io/github/issues/Thundernerd/Unity3D-EnumAsIndex" />
	</a>
	<a href="https://github.com/Thundernerd/Unity3D-EnumAsIndex/pulls">
		<img alt="GitHub pull requests" src ="https://img.shields.io/github/issues-pr/Thundernerd/Unity3D-EnumAsIndex" />
	</a>
	<a href="https://github.com/Thundernerd/Unity3D-EnumAsIndex/blob/master/LICENSE.md">
		<img alt="GitHub license" src ="https://img.shields.io/github/license/Thundernerd/Unity3D-EnumAsIndex" />
	</a>
	<img alt="GitHub last commit" src ="https://img.shields.io/github/last-commit/Thundernerd/Unity3D-EnumAsIndex" />
</p>

An attribute that allows you to use an enum as an index for arrays and lists.

## Installation
1. The package is available on the [openupm registry](https://openupm.com). You can install it via [openupm-cli](https://github.com/openupm/openupm-cli).
```
openupm add net.tnrd.enumasindex
```
2. You can also install via git url by adding these entries in your **manifest.json**
```json
"net.tnrd.enumasindex": "https://github.com/Thundernerd/Unity3D-EnumAsIndex.git",
"net.tnrd.constrainedrect": "https://github.com/Thundernerd/Unity3D-ConstrainedRect.git"
```

## Usage

Usage is easy, let's assume we have the following enum:

```c#
public enum MyEnum
{
    Hello,
    World,
    Goodbye,
    Foo,
    Bar
}
```

We can apply the EnumAsIndex attribute easily to both arrays and lists as shown below, using typeof with the enum we want to use as an index.

In the Awake method you can access the value in the list by converting the enum value to an integer to serve as an index.

```c#
public class Dummy : MonoBehaviour
{
    [SerializeField, EnumAsIndex(typeof(MyEnum))] private List<int> fooList;
    [SerializeField, EnumAsIndex(typeof(MyEnum))] private int[] barArray;

    private void Awake()
    {
        var item = fooList[(int) MyEnum.Goodbye];
    }
}
```

The result in the editor will be as below. Note that you **need to change to size manually once** for it to work. Afterwards it'll automatically update the size.

![in-editor example](./~Documentation/dummy.png)

You cannot modify the value of the dropdown boxes. They are decorative and only serve to show you what index the value is for.

## Support
**Enum As Index** is a small and open-source utility that I hope helps other people. It is by no means necessary but if you feel generous you can support me by donating.

[![ko-fi](https://www.ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/J3J11GEYY)

## Contributions
Pull requests are welcomed. Please feel free to fix any issues you find, or add new features.

