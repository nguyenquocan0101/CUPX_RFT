// See https://aka.ms/new-console-template for more information
using DynamicExpresso;

var foo = new Foo { Side = 1, Demo1 = 0};

var conditions = new List<ConditionModel>
{
    new ConditionModel
    {
        Name = "Side",
        Description = "string",
        Expression = "Side == 1"
    },
    new ConditionModel
     {
        Name = "Demo1",
        Description = "string",
        Expression = "Demo1 == 1"
    }
};

Console.WriteLine($"Thuộc tính: {nameof(foo.Side)}, Giá trị thuộc tính: {foo.Side}");
Console.WriteLine($"Thuộc tính: {nameof(foo.Demo1)}, Giá trị thuộc tính: {foo.Demo1}");


bool isMatch = EvaluateAllConditions(foo, conditions);
Console.WriteLine($"Kết quả: {isMatch}");



static bool EvaluateAllConditions(object obj, List<ConditionModel> conditions)
{
    var type = obj.GetType();
    var interpreter = new Interpreter();

    foreach (var prop in type.GetProperties())
    {
        var value = prop.GetValue(obj);
        interpreter.SetVariable(prop.Name, value, prop.PropertyType);
    }
    foreach (var condition in conditions)
    {
        try
        {
            var result = interpreter.Eval(condition.Expression);
            if (result is not bool b || b == false)
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi khi đánh giá biểu thức: {condition.Expression}");
            Console.WriteLine($"Lý do: {ex.Message}");
            return false;
        } 
    }
    return true; 
}


public class Foo
{
    public int Demo1 { get; set; }
    public int Demo2 { get; set; }
    public int Side { get; set; }
}


public class ConditionModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Expression { get; set; }
}

