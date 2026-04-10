# Excel Converter Design Spec

**Goal:** Unity용 Excel → GameData 변환 모듈. 리플렉션으로 시트/컬럼 자동 대응.

**Architecture:** 
- ExcelConverter<T> 제네릭 클래스로 GameData 변환
- SheetAttribute: 시트 이름 강제 대응
- IgnoreAttribute: 필드 무시
- 필드명 = 시트명/컬럼명 자동 대응

**Tech Stack:** Unity, C#, ExcelDataReader 라이브러리

---

## Components

### 1. SheetAttribute
```csharp
[AttributeUsage(AttributeTargets.Field)]
public class SheetAttribute : Attribute {
    public string Name { get; }
    public SheetAttribute(string name) { Name = name; }
}
```

### 2. IgnoreAttribute
```csharp
[AttributeUsage(AttributeTargets.Field)]
public class IgnoreAttribute : Attribute { }
```

### 3. ExcelConverter<T>
```csharp
public static class ExcelConverter {
    public static T Load<T>(string filePath) where T : new();
    // StreamingAssets 경로에서 .xlsx 파일 읽어서 T로 변환
    
    private static List<object> ParseSheet(Type rowType, DataTable sheet);
    // 시트의 각 Row를 rowType 객체로 변환
    
    private static object ParseRow(Type type, DataRow row, Dictionary<string, int> columnMap);
    // Row 데이터를 type 객체로 리플렉션 변환
}
```

---

## Mapping Rules

### GameData (시트 → 필드)
| 필드 | 시트 이름 |
|------|----------|
| `public List<WeaponData> weapons;` | "weapons" (필드명) |
| `[Sheet("Items")] public List<ItemData> items;` | "Items" (강제) |
| `[Ignore] public long gold;` | 무시 |

### Data Class (컬럼 → 필드)
| 필드 | 컬럼 이름 |
|------|----------|
| `public int Id;` | "Id" (필드명) |
| `public string Name;` | "Name" (필드명) |
| `[Ignore] public string temp;` | 무시 |

---

## Supported Types

- `int`
- `long`
- `float`
- `double`
- `string`
- `bool`

---

## File Structure

```
Assets/Scripts/ExcelConverter/
├── Attributes.cs        # SheetAttribute, IgnoreAttribute
├── ExcelConverter.cs    # 메인 변환 로직
└── Editor/
    └── ExcelConverterEditor.cs  # 에디터 메뉴 (선택)
    
Assets/StreamingAssets/
└── GameData.xlsx        # Excel 파일 위치
```

---

## Usage

### Runtime
```csharp
var gameData = ExcelConverter.Load<GameData>("GameData.xlsx");
```

### Editor Menu
```csharp
// Tools > Excel Converter > Convert to Asset
ExcelConverter.Load<GameData>("GameData.xlsx");
// ScriptableObject로 저장
```

---

## Data Flow

1. `ExcelConverter.Load<T>(filePath)` 호출
2. StreamingAssets에서 .xlsx 파일 로드
3. ExcelDataReader로 시트들 읽기
4. T의 List<TData> 필드 찾기
5. 각 필드에 대응하는 시트 이름으로 시트 찾기
6. 시트의 첫 번째 Row → 컬럼 이름 맵 생성
7. 각 Row → TData 객체로 리플렉션 변환
8. List<TData>에 추가
9. T 반환

---

## Edge Cases

- 시트 없음: Warning 로그, List 빈 상태
- 컬럼 없음: Warning 로그, 필드 default 값
- 타입 변환 실패: Warning 로그, default 값
- Excel 파일 없음: Error 로그, null 반환

---

## Dependencies

- ExcelDataReader (NuGet)
  - `ExcelDataReader`
  - `ExcelDataReader.DataSet`
  - Plugins 폴더에 복사 필요