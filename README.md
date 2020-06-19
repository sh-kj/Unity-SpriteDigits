# Unity Sprite Digits

Number Display System Using 0 to 9 Sprites.  
You can use your original number images to display digits.

Digits are scaled by RectTransform to they are always inside Rect, and Horizontal/Vertical Pivot setting are available.

# Usage

## Prepare Number Sprites

First, you have to create `Digits` ScriptableObject. It can do with Project context menu.

Number Sprites' Size must be 10. You have to prepare all 0 to 9 sprite and Minus sprite, Decimal Point sprite is necessary when you display float number.

## Display Integer Number

Create GameObject and add SpriteDigits component.

Set your Digits ScriptableObject to `Digits` Inspector parameter to display numbers.  
after that, you can change `Value` parameter to change numbers.

### Parameters

- MaxDigitNum  
0 is infinite. if this is larger than 1, Digits are counter-stop with max display value(ex:99999).
- Padding Mode  
If you set it Zero-Fill and MaxDigitNum to Non-Zero, It fills 0 to upper digits(ex:00123).

## Display Float Number

Create GameObject and add SpriteDigitsFloat component, and set `Digits` too.

`Value`'s Type is Double now, and it displays fixed-point Digits.

### Parameters

- DisplayDecimalPlaces  
Number of Digits After Decimal Point.

## Common Paremeters

- Color  
Sprites' vertex color.
- SortingLayerID, OrderInLayer  
You can change them to Sprite sorting order.
- Custom Material  
If you want to change material, your shader would be based on `Sprites/Default`.  
- Size  
Set it to change scale. But scale is Maximized by Rect.
- Spacing  
Set it to change Space between digits.
- Horizontal/Vertical Pivot  
Set it to change Digits' Position inside Rect.

# License

MIT