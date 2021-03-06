﻿/*
 * Copyright (C) 2011 - 2018 mooege project
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Data;

namespace Mooege.Common.Storage.AccountDataBase.Entities
{
    public class DBInventory : Entity
    {
        public new virtual ulong Id { get; set; }
        public virtual DBGameAccount DBGameAccount { get; set; }
        public virtual DBToon DBToon { get; set; }
        public virtual int LocationX { get; set; }
        public virtual int LocationY { get; set; }
        public virtual int EquipmentSlot { get; set; }
        //public virtual int ItemId { get; set; }
        public virtual DBItemInstance DBItemInstance { get; set; }

    }
}
