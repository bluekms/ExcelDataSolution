namespace Sdp.Attributes;

// DataAnnotations 상속은 netstandard2.1(유니티)에 없는 어셈블리 의존을 만들고,
// SG 는 생성자 인자만 읽으므로 독립 attribute 로 충분하다.
[AttributeUsage(AttributeTargets.Parameter)]
public class RegularExpressionAttribute(string pattern) : Attribute
{
    public string Pattern { get; } = pattern;
}
