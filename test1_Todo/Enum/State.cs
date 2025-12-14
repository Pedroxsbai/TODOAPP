namespace test1_Todo.Enum
{
   // [Flags] // si jamais on peut choisir plusieurs State en même temps
    public enum State
    {
        Todo = 0,
        Doing = 1,            
        Done = 2,               
                        // 1 = 2^0
                        // 2 = 2^1
                       // 4 = 2^2                       // on doit toujours mettre des puissances de 2 (1,2,4,8,16,32,...)
    }
}
