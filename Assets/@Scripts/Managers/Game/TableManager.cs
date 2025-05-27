using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TableManager
{
    public TableManager()
    {
        Debug.Log("<color=orange>[TableManager]</color> 생성됨");
    }
    private List<Table> _tables = new List<Table>();
    public IReadOnlyList<Table> Tables => _tables;

    public void SetInfo(){
        
    Managers.Subscribe(ActionType.Chair_OccupiedChanged, () => {
                foreach (var table in _tables)
                {
                    if (table.IsFullyOccupied)
                    {
                        Managers.PublishAction(ActionType.Customer_TableFullyOccupied);
                    }
                }
        });
    }
    public void RegisterTable(Table table)
    {
        if (!_tables.Contains(table))
            _tables.Add(table);
    }

    public void UnregisterTable(Table table)
    {
        if (_tables.Contains(table))
            _tables.Remove(table);
    }

   
}