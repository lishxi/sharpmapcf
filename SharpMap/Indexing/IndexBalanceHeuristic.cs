// Copyright 2006, 2007 - Rory Plaire (codekaizen@gmail.com)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMap.Indexing
{
    /// <summary>
    /// Heuristics used for tree generation and maintenance.
    /// </summary>
    public abstract class IndexBalanceHeuristic
    {
        private int _nodeItemMin;
        private int _nodeItemMax;
        private uint _maxTreeDepth;

        protected IndexBalanceHeuristic(int nodeItemMinimumCount, int nodeItemMaximumCount, uint maxTreeDepth)
        {
            _nodeItemMin = nodeItemMinimumCount;
            _nodeItemMax = nodeItemMaximumCount;
            _maxTreeDepth = maxTreeDepth;
        }

        /// <summary>
        /// Minimum number of index entries in a node before it is a candiate for splitting
        /// the node.
        /// </summary>
        public virtual int NodeItemMinimumCount
        {
            get { return _nodeItemMin; }
        }

        /// <summary>
        /// Number of index entries in a node to target. More than this will cause a split
        /// if <see cref="MaxTreeDepth"/> is not reached.
        /// </summary>
        public virtual int NodeItemMaximumCount
        {
            get { return _nodeItemMax; }
        }

        /// <summary>
        /// The maximum depth of the tree including the root.
        /// </summary>
        public virtual uint MaxTreeDepth
        {
            get { return _maxTreeDepth; }
        }

        /// <summary>
        /// The target number of nodes for a split node.
        /// </summary>
        public abstract int TargetNodeCount { get; }
    }
}
