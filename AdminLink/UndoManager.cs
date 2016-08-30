using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace Nixxis.Client
{
    public interface IDisplayTextProvider
    {
        string TypedDisplayText { get; }
        string DisplayText { get; }
        string ShortDisplayText { get; }
    }

    public abstract class UndoOperation
    {
        public static readonly string Add = "Add";
        public static readonly string Change = "Change";
        public static readonly string Delete = "Delete";

        public string Description { get; protected set; }

        private UndoOperation()
        {
        }

        public UndoOperation(string description)
        {
            Description = description;
        }

        public UndoOperation(string operation, string item)
            : this(string.Concat(operation, " ", item))
        {
        }

        public UndoOperation(string operation, IDisplayTextProvider item)
            : this(string.Concat(operation, " ", item.TypedDisplayText))
        {
        }

        protected abstract void Undo();

        internal void __Undo()
        {
            Undo();
        }
    }

    public class UndoManager : INotifyPropertyChanged
    {
        private class Transaction : UndoOperation
        {
            private Stack<UndoOperation> m_Operations = new Stack<UndoOperation>();
            private bool m_Aborted;

            public Transaction(string description)
                : base(description)
            {
            }

            public Transaction(string operation, string item)
                : base(operation, item)
            {
            }

            public Transaction(string operation, IDisplayTextProvider item)
                : base(operation, item)
            {
            }

            public bool Aborted
            {
                get
                {
                    return m_Aborted;
                }
            }

            protected override void Undo()
            {
                while (m_Operations.Count > 0)
                {
                    m_Operations.Pop().__Undo();
                }

                m_Aborted = true;
                m_Operations = null;
            }

            public void Push(UndoOperation operation)
            {
                m_Operations.Push(operation);
            }
        }

        private class TransactionReferrer : IDisposable
        {
            UndoManager m_Manager;
            Transaction m_Transaction;

            public TransactionReferrer(UndoManager manager, Transaction transaction)
            {
                m_Manager = manager;
                m_Transaction = transaction;
            }

            public Transaction Transaction
            {
                get
                {
                    return m_Transaction;
                }
            }

            public void Dispose()
            {
                m_Manager.ReferrerDisposed(this);
            }
        }

        public static UndoManager m_GlobalManager;

        public static UndoManager Global
        {
            get
            {
                lock (typeof(UndoManager))
                {
                    if (m_GlobalManager == null)
                        m_GlobalManager = new UndoManager();

                    return m_GlobalManager;
                }
            }
        }

        private int m_StackSize;
        private List<UndoOperation> m_UndoList = new List<UndoOperation>();
        private Stack<Transaction> m_ActiveTransactions = new Stack<Transaction>();
        private SynchronizationContext m_SyncContext;

        public UndoManager()
            : this(100)
        {
        }

        public UndoManager(int stackSize)
        {
            this.StackSize = stackSize;
            m_SyncContext = SynchronizationContext.Current as SynchronizationContext;
        }

        public int StackSize
        {
            get
            {
                return m_StackSize;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException();

                lock (m_UndoList)
                {
                    m_StackSize = value;

                    if (m_UndoList.Count > m_StackSize)
                    {
                        m_UndoList.RemoveRange(0, m_UndoList.Count - m_StackSize);
                    }
                }
            }
        }

        public IDisposable BeginTransaction(string description)
        {
            lock(m_UndoList)
            {
                Transaction NewTransaction = new Transaction(description);

                m_ActiveTransactions.Push(NewTransaction);

                if (m_ActiveTransactions.Count == 1 && m_UndoList.Count > 0)
                    FirePropertyChanged(new PropertyChangedEventArgs("CanUndo"));

                return new TransactionReferrer(this, NewTransaction);
            }
        }

        public IDisposable BeginTransaction(string operation, string item)
        {
            lock (m_UndoList)
            {
                Transaction NewTransaction = new Transaction(operation, item);

                m_ActiveTransactions.Push(NewTransaction);

                if (m_ActiveTransactions.Count == 1 && m_UndoList.Count > 0)
                    FirePropertyChanged(new PropertyChangedEventArgs("CanUndo"));
                
                return new TransactionReferrer(this, NewTransaction);
            }
        }

        public IDisposable BeginTransaction(string operation, IDisplayTextProvider item)
        {
            lock (m_UndoList)
            {
                Transaction NewTransaction = new Transaction(operation, item);

                m_ActiveTransactions.Push(NewTransaction);

                if (m_ActiveTransactions.Count == 1 && m_UndoList.Count > 0)
                    FirePropertyChanged(new PropertyChangedEventArgs("CanUndo"));

                return new TransactionReferrer(this, NewTransaction);
            }
        }

        public void CommitTransaction()
        {
            lock (m_UndoList)
            {
                Push(m_ActiveTransactions.Pop());

                if (m_ActiveTransactions.Count == 0)
                    FirePropertyChanged(new PropertyChangedEventArgs("CanUndo"));
            }
        }

        public void RollbackTransaction()
        {
            lock (m_UndoList)
            {
                m_ActiveTransactions.Pop().__Undo();

                if(m_ActiveTransactions.Count == 0 && m_UndoList.Count > 0)
                    FirePropertyChanged(new PropertyChangedEventArgs("CanUndo"));
            }
        }

        private void ReferrerDisposed(TransactionReferrer referrer)
        {
            lock (m_UndoList)
            {
                if (!m_ActiveTransactions.Peek().Equals(referrer.Transaction))
                    throw new InvalidOperationException();

                if (!referrer.Transaction.Aborted)
                    CommitTransaction();
            }
        }

        public void Push(UndoOperation operation)
        {
            lock (m_UndoList)
            {
                if (m_ActiveTransactions.Count == 0)
                {
                    if (m_StackSize > 0)
                    {
                        while (m_UndoList.Count >= m_StackSize)
                            m_UndoList.RemoveAt(0);

                        m_UndoList.Add(operation);

                        FirePropertyChanged(new PropertyChangedEventArgs("Descriptions"));

                        if(m_UndoList.Count == 1)
                            FirePropertyChanged(new PropertyChangedEventArgs("CanUndo"));
                    }
                }
                else
                {
                    m_ActiveTransactions.Peek().Push(operation);
                }
            }
        }

        public void Undo()
        {
            Undo(1);
        }

        public void Undo(int level)
        {
            lock (m_UndoList)
            {
                for (int i = 0; i < level; i++)
                {
                    int Index = m_UndoList.Count - 1;
                    
                    if (Index < 0)
                        break;

                    m_UndoList[Index].__Undo();

                    m_UndoList.RemoveAt(Index);
                }

                FirePropertyChanged(new PropertyChangedEventArgs("Descriptions"));

                if (m_UndoList.Count == 0)
                    FirePropertyChanged(new PropertyChangedEventArgs("CanUndo"));
            }
        }

        public bool CanUndo
        {
            get
            {
                lock (m_UndoList)
                {
                    return (m_ActiveTransactions.Count == 0 && m_UndoList.Count > 0);
                }
            }
        }

        public string[] Descriptions
        {
            get
            {
                lock (m_UndoList)
                {
                    string[] Descs = new string[m_UndoList.Count];

                    for (int i = 0; i < m_UndoList.Count; i++)
                    {
                        Descs[i] = m_UndoList[m_UndoList.Count - (i + 1)].Description;
                    }

                    return Descs;
                }
            }
        }

        protected void FirePropertyChanged(PropertyChangedEventArgs args)
        {
            if (m_SyncContext.Equals(SynchronizationContext.Current))
                m_SyncContext.Post(SyncOnPropertyChanged, args);
            else
                OnPropertyChanged(args);
        }

        private void SyncOnPropertyChanged(object state)
        {
            OnPropertyChanged((PropertyChangedEventArgs)state);
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, args);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}