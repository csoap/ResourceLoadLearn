﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : Singleton<ResourceManager> {

}

//双向链表结构节点
public class DoubleLinkedListNode<T> where T : class, new()
{
    //前一个节点
    public DoubleLinkedListNode<T> prev = null;
    //后一个节点
    public DoubleLinkedListNode<T> next = null;
    //当前节点
    public T t = null;
}

// 双向链表结构
public class DoubleLinkedList<T> where T : class, new()
{
    //表头
    public DoubleLinkedListNode<T> Head = null;
    //表尾
    public DoubleLinkedListNode<T> Tail = null;
    //双向链表结构类对象池
    protected ClassObjectPool<DoubleLinkedListNode<T>> m_DoubleLinkNodePool = ObjectManager.Instance.GetOrCreateClassPool<DoubleLinkedListNode<T>>(500);
    //个数
    protected int m_Count = 0;
    public int Count
    {
        get { return m_Count; }
    }

    /// <summary>
    /// 添加一个节点到头部
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public DoubleLinkedListNode<T> AddToHead(T t)
    {
        DoubleLinkedListNode<T> pList = m_DoubleLinkNodePool.Spwan(true);
        pList.next = null;
        pList.prev = null;
        pList.t = t;
        return AddToHeadr(pList);
    }

    /// <summary>
    /// 添加一个节点到头部
    /// </summary>
    /// <param name="pNode"></param>
    /// <returns></returns>
    public DoubleLinkedListNode<T> AddToHeadr(DoubleLinkedListNode<T> pNode)
    {
        if (pNode == null) return null;
        pNode.prev = null;
        if (Head == null)
        {
            Head = Tail = pNode;
        }
        else
        {
            pNode.next = Head;
            Head.prev = pNode;
            Head = pNode;
        }
        m_Count++;
        return Head;
    }

    /// <summary>
    /// 添加节点到尾部
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public DoubleLinkedListNode<T> AddToTail(T t)
    {
        DoubleLinkedListNode<T> pList = m_DoubleLinkNodePool.Spwan(true);
        pList.next = null;
        pList.prev = null;
        pList.t = t;
        return AddToTail(pList);
    }

    /// <summary>
    /// 添加节点到尾部
    /// </summary>
    /// <param name="pNode"></param>
    /// <returns></returns>
    public DoubleLinkedListNode<T> AddToTail(DoubleLinkedListNode<T> pNode)
    {
        if (pNode == null) return null;
        pNode.next = null;
        if (Tail ==null)
        {
            Head = Tail = pNode;
        }
        else
        {
            pNode.prev = Tail;
            Tail.next = pNode;
            Tail = pNode;
        }
        m_Count++;
        return Head;
    }
    
    /// <summary>
    /// 移除节点
    /// </summary>
    /// <param name="pNode"></param>
    public void RemoveNode(DoubleLinkedListNode<T> pNode)
    {
        if (pNode == null) return;
        if (pNode == Head)
        {
            Head = pNode.next;
        }
        if (pNode == Tail)
        {
            Tail = pNode.prev;
        }
        if (pNode.prev != null)
        {
            pNode.prev.next = pNode.next;
        }
        if (pNode.next != null)
        {
            pNode.next.prev = pNode.prev;
        }
        pNode.next = pNode.prev = null;
        pNode.t = null; 
        m_Count--;
        m_DoubleLinkNodePool.Recycle(pNode);
    }

    public void MoveToHead(DoubleLinkedListNode<T> pNode)
    {
        if (pNode == null || pNode == Head)
        {
            return;
        }
        if (pNode.prev == null && pNode.next == null)
        {
            //回收了
            return;
        }
        if (pNode == Tail)
        {
            Tail = pNode.prev;
        }
        if (pNode.prev != null)
        {
            pNode.prev.next = pNode.next;
        }
        if (pNode.next != null)
        {
            pNode.next.prev = pNode.prev;
        }
        pNode.prev = null;
        pNode.next = Head;
        Head.prev = pNode;
        Head = pNode;
        if (Tail == null)
        {
            Tail = Head;
        }
    }
}

public class CMapList<T> where T : class, new()
{
    DoubleLinkedList<T> m_DLink = new DoubleLinkedList<T>();
}