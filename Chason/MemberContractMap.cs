//--------------------------------------------------------------------------------------------------
// <copyright file="MemberContractMap.cs" company="Andrew Chisholm">
//   Copyright (c) 2013 Andrew Chisholm All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace Chason
{
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;

    /// <summary>
    /// Mapping between a Member and a DataContract
    /// </summary>
    public struct MemberContractMap
    {

        /// <summary>
        /// </summary>
        private MemberInfo member;

        private readonly int sortOrder;

        /// <summary>
        /// </summary>
        private DataMemberAttribute contract;

        public MemberContractMap(MemberInfo member, int sortOrder)
        {
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }

            this.member = member;
            this.sortOrder = sortOrder;
            this.contract = null;
        }


        public MemberContractMap(MemberInfo member, DataMemberAttribute contract)
        {
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }

            if (contract == null)
            {
                throw new ArgumentNullException("contract");
            }

            this.member = member;
            this.contract = contract;
            this.sortOrder = this.contract.Order;
        }

        public MemberInfo Member
        {
            get
            {
                return this.member;
            }
        }

        public DataMemberAttribute Contract
        {
            get
            {
                return this.contract;
            }
        }

        public string Name
        {
            get
            {
                if (this.contract != null)
                {
                    return this.contract.Name ?? this.member.Name;
                }

                return this.member.Name;
            }
        }

        public int SortOrder
        {
            get
            {
                return this.sortOrder;
            }
        }


    }
}
