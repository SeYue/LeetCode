// https://leetcode.cn/problems/add-two-numbers/description/
// 解题思路:递归

public class 两数相加
{
	public 两数相加()
	{
		Console.WriteLine("两数相加");

		ListNode l1 = new ListNode(2, new ListNode(4, new ListNode(3)));
		ListNode l2 = new ListNode(5, new ListNode(6, new ListNode(4)));
		ListNode result = AddTwoNumbers(l1, l2);
		ListNode tmp = result;
		while (tmp != null)
		{
			Console.WriteLine(tmp.val);
			tmp = tmp.next;
		}

		Console.WriteLine("两数相加");

		l1 = new ListNode(9, new ListNode(9, new ListNode(9, new ListNode(9, new ListNode(9, new ListNode(9, new ListNode(9)))))));
		l2 = new ListNode(9, new ListNode(9, new ListNode(9, new ListNode(9))));

		Console.WriteLine("开始计算");
		result = AddTwoNumbers(l1, l2);
		Console.WriteLine("结束计算");

		tmp = result;
		while (tmp != null)
		{
			Console.WriteLine(tmp.val);
			tmp = tmp.next;
		}
	}

	public ListNode AddTwoNumbers(ListNode l1, ListNode l2)
	{
		return AddToNode(l1, l2, 0);
	}

	static ListNode AddToNode(ListNode l1, ListNode l2, int carryByte)
	{
		ListNode result = new ListNode(carryByte);
		ListNode l1Next = null;
		ListNode l2Next = null;
		if (l1 != null)
		{
			result.val += l1.val;
			l1Next = l1.next;
		}
		if (l2 != null)
		{
			result.val += l2.val;
			l2Next = l2.next;
		}
		carryByte = result.val / 10;
		result.val = result.val % 10;

		if (l1Next != null || l2Next != null || carryByte > 0)
		{
			result.next = AddToNode(l1Next, l2Next, carryByte);
		}
		return result;
	}
}

public class ListNode
{
	public int val;
	public ListNode next;
	public ListNode(int val = 0, ListNode next = null)
	{
		this.val = val;
		this.next = next;
	}
}

/**
 * Definition for singly-linked list.
 * public class ListNode {
 *     public int val;
 *     public ListNode next;
 *     public ListNode(int val=0, ListNode next=null) {
 *         this.val = val;
 *         this.next = next;
 *     }
 * }
 */